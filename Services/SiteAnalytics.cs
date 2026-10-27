using System.Security.Cryptography;
using System.Text;
using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services;

/// <summary>Persists a local secret, not raw addresses. Configure trusted proxies before using host counts behind a proxy.</summary>
public sealed class AnalyticsHostHasher
{
    private readonly byte[] key;
    public AnalyticsHostHasher(IWebHostEnvironment environment)
    {
        var directory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "analytics-host.key");
        if (!File.Exists(file))
        {
            try
            {
                using var stream = new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                stream.Write(RandomNumberGenerator.GetBytes(32));
            }
            catch (IOException) when (File.Exists(file)) { }
        }
        key = File.ReadAllBytes(file);
        if (key.Length != 32) throw new InvalidOperationException("Invalid analytics host key.");
    }
    public string? Hash(HttpContext http, DateTime utc)
    {
        var ip = http.Connection.RemoteIpAddress;
        if (ip is null) return null;
        var value = utc.AddHours(3).ToString("yyyy-MM-dd") + "|" + ip.MapToIPv6();
        return Convert.ToHexString(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(value)));
    }
}

public static class SiteAnalytics
{
    private static readonly HashSet<string> Kinds = new(StringComparer.Ordinal)
    { "page_view", "phone_click", "calculator", "form_start", "form_attempt", "form_invalid", "form_error", "block_view" };

    public static void MapSiteAnalytics(this WebApplication app)
    {
        app.MapPost("/api/site-event", async Task<IResult> (HttpContext http, SiteEventInput input,
            IDbContextFactory<AppDBContext> factory, AnalyticsHostHasher hasher, ILoggerFactory loggers) =>
        {
            if (http.Request.ContentLength is > 8192 || input.Id == Guid.Empty ||
                input.VisitId == Guid.Empty || input.VisitorId == Guid.Empty || !Kinds.Contains(input.Kind ?? ""))
                return Results.BadRequest();
            if (http.User.Identity?.IsAuthenticated == true) return Results.NoContent();
            if (http.Request.Headers["Sec-Fetch-Site"] == "cross-site") return Results.StatusCode(403);
            var origin = http.Request.Headers["Origin"].ToString();
            if (origin.Length > 0 && (!Uri.TryCreate(origin, UriKind.Absolute, out var o) ||
                !string.Equals(o.Authority, http.Request.Host.Value, StringComparison.OrdinalIgnoreCase)))
                return Results.StatusCode(403);
            var agent = http.Request.Headers.UserAgent.ToString();
            if (System.Text.RegularExpressions.Regex.IsMatch(agent, "bot|crawler|spider|headless|slurp", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return Results.NoContent();
            var path = input.Path ?? "/";
            if (path.Length > 300 || !path.StartsWith('/') || path.StartsWith("//") ||
                path.Contains('?') || path.Contains('#')) return Results.BadRequest();
            try
            {
                await using var db = await factory.CreateDbContextAsync();
                var slug = path.Trim('/');
                City? city = null;
                if (slug.Length > 0 && slug is not "contacts" and not "user-agreement")
                {
                    if (slug.Length > 50 || slug.Any(c => !(c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-')))
                        return Results.BadRequest();
                    city = await db.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
                    if (city is null) return Results.BadRequest();
                }
                await using var tx = await db.Database.BeginTransactionAsync();
                await db.Database.ExecuteSqlInterpolatedAsync($@"
                    DECLARE @result int;
                    EXEC @result=sys.sp_getapplock @Resource={"ecobay:analytics:" + input.VisitId.ToString("N")},
                        @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=3000;
                    IF @result < 0 THROW 51001, 'Analytics busy', 1;");
                var now = DateTime.UtcNow;
                var visit = await db.AnalyticsVisits.SingleOrDefaultAsync(v => v.Id == input.VisitId);
                if (visit is not null && visit.VisitorId != input.VisitorId) return Results.BadRequest();
                if (await db.AnalyticsEvents.AnyAsync(e => e.Id == input.Id)) return Results.NoContent();
                if (visit is null)
                {
                    var referrer = ReferrerHost(input.Referrer, http.Request.Host.Host);
                    visit = new AnalyticsVisit {
                        Id = input.VisitId, VisitorId = input.VisitorId, StartedAtUtc = now, LastSeenAtUtc = now,
                        CityId = city?.Id, CityName = city?.Name ?? "Общая страница", LandingPath = path,
                        Source = Classify(input, referrer), Campaign = Clean(input.Campaign, 200),
                        Content = Clean(input.Content, 200), Term = Clean(input.Term, 200), ReferrerHost = referrer,
                        Device = agent.Contains("iPad", StringComparison.OrdinalIgnoreCase) || agent.Contains("Tablet", StringComparison.OrdinalIgnoreCase)
                            ? "tablet" : agent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ? "mobile" : "desktop",
                        Browser = Browser(agent)
                    };
                    db.AnalyticsVisits.Add(visit);
                }
                else visit.LastSeenAtUtc = now;
                var calc = input.Kind == "calculator";
                if (calc && (input.Weight is null or <= 0 or > 1000000 ||
                    input.AmountFrom is < 0 or > 100000000 || input.AmountTo is < 0 or > 100000000))
                    return Results.BadRequest();
                db.AnalyticsEvents.Add(new AnalyticsEvent {
                    Id = input.Id, VisitId = input.VisitId, CreatedAtUtc = now, Kind = input.Kind!,
                    Path = path, CityId = city?.Id, DailyHost = hasher.Hash(http, now),
                    Location = Clean(input.Location, 40), Weight = calc ? input.Weight : null,
                    AmountFrom = calc ? input.AmountFrom : null, AmountTo = calc ? input.AmountTo : null
                });
                if (input.Kind == "phone_click")
                    db.PhoneClicks.Add(new PhoneClick { Id = input.Id, Path = path, CityId = city?.Id,
                        CityName = city?.Name ?? "Общая страница", CreatedAtUtc = now });
                await db.SaveChangesAsync();
                await tx.CommitAsync();
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                loggers.CreateLogger("SiteAnalytics").LogWarning(ex, "Analytics event was not saved");
                return Results.StatusCode(503);
            }
        });
    }
    private static string Clean(string? value, int max) =>
        new((value ?? "").Trim().Where(c => !char.IsControl(c)).Take(max).ToArray());
    private static string ReferrerHost(string? value, string ownHost)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http")) return "";
        return uri.Host.Equals(ownHost, StringComparison.OrdinalIgnoreCase) ? "" : Clean(uri.Host.ToLowerInvariant(), 253);
    }
    private static bool Domain(string host, string domain) => host == domain || host.EndsWith("." + domain, StringComparison.Ordinal);
    public static string Classify(SiteEventInput input, string host)
    {
        var source = (input.UtmSource ?? "").Trim().ToLowerInvariant();
        var medium = (input.UtmMedium ?? "").Trim().ToLowerInvariant();
        var paid = medium is "cpc" or "ppc" or "paid" or "paidsearch" or "display" or "cpm" or "retargeting";
        if (source == "yandex" && paid) return "yandex_ads";
        if (source == "google" && paid) return "google_ads";
        if (input.Yclid) return "yandex_ads";
        if (input.Gclid) return "google_ads";
        // Explicit other campaigns must not be silently labelled organic search.
        if (source.Length > 0 && medium != "organic") return "other_campaign";
        if (Domain(host, "yandex.ru") || Domain(host, "yandex.by") || Domain(host, "ya.ru") ||
            Domain(host, "yandex.com") || source == "yandex" && medium == "organic") return "yandex_search";
        if (Domain(host, "google.com") || Domain(host, "google.by") || Domain(host, "google.ru") ||
            Domain(host, "google.co.uk") || source == "google" && medium == "organic") return "google_search";
        return host.Length > 0 ? "referral" : "direct";
    }
    public static string Title(string source) => source switch {
        "yandex_ads" => "Яндекс Директ", "google_ads" => "Google Реклама",
        "yandex_search" => "Яндекс: поиск", "google_search" => "Google: поиск",
        "referral" => "Другие сайты", "other_campaign" => "Другие кампании",
        _ => "Прямой / не определён"
    };
    private static string Browser(string a) => a.Contains("Edg/") ? "Edge" : a.Contains("YaBrowser/") ? "Яндекс Браузер" :
        a.Contains("OPR/") ? "Opera" : a.Contains("Firefox/") ? "Firefox" :
        a.Contains("Chrome/") || a.Contains("CriOS/") ? "Chrome" : a.Contains("Safari/") ? "Safari" : "Другой";
}

using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services;

public sealed class TrafficNumbers
{
    public int Visits { get; set; }
    public int Visitors { get; set; }
    public int Views { get; set; }
    public int CalcVisits { get; set; }
    public int Calculations { get; set; }
    public int Calls { get; set; }
    public int CallVisits { get; set; }
    public int Starts { get; set; }
    public int Attempts { get; set; }
    public int Invalid { get; set; }
    public int Errors { get; set; }
    public int Requests { get; set; }
    public int RequestVisits { get; set; }
    public int ContactVisits { get; set; }
    public int CalcContacts { get; set; }
    public decimal Conversion => Visits == 0 ? 0 : 100m * ContactVisits / Visits;
}
public sealed record TrafficGroup(string Name, TrafficNumbers Numbers);
public sealed record TrafficDay(DateTime Day, TrafficNumbers Numbers, int Hosts);
public sealed record TrafficSourceDay(DateTime Day, string Source, int Visits);
public sealed record TrafficCount(string Name, int Count);
public sealed class TrafficReport
{
    public TrafficNumbers Total { get; set; } = new();
    public List<TrafficGroup> Sources { get; set; } = new();
    public List<TrafficGroup> Cities { get; set; } = new();
    public List<TrafficGroup> Campaigns { get; set; } = new();
    public List<TrafficGroup> Devices { get; set; } = new();
    public List<TrafficCount> Pages { get; set; } = new();
    public List<TrafficCount> Phones { get; set; } = new();
    public List<TrafficCount> Blocks { get; set; } = new();
    public List<TrafficCount> Weights { get; set; } = new();
    public List<TrafficCount> Referrers { get; set; } = new();
    public List<TrafficCount> Browsers { get; set; } = new();
    public List<TrafficDay> Days { get; set; } = new();
    public List<TrafficSourceDay> SourceDays { get; set; } = new();
    public DateTime? InstalledAtUtc { get; set; }
}
public sealed record TrafficFilter(DateTime From, DateTime To, long? CityId = null,
    string Source = "", string Campaign = "", string Term = "", string Device = "");

/// <summary>Bounded visit projections and SQL-grouped events, never loads personal client/form data.</summary>
public sealed class AnalyticsReports(IDbContextFactory<AppDBContext> factory, SitePermissions permissions)
{
    private sealed class EventGroup
    {
        public Guid VisitId { get; set; }
        public string Kind { get; set; } = "";
        public string Location { get; set; } = "";
        public string Path { get; set; } = "";
        public int Count { get; set; }
    }
    public static DateTime MinskToday => DateTime.UtcNow.AddHours(3).Date;
    public static DateTime UtcStart(DateTime day) => DateTime.SpecifyKind(day.Date.AddHours(-3), DateTimeKind.Utc);

    public async Task<TrafficReport> LoadAsync(TrafficFilter filter)
    {
        await permissions.RequireStaffAsync();
        if (filter.To.Date < filter.From.Date || (filter.To.Date - filter.From.Date).Days > 365)
            throw new ArgumentException("Выберите период не более 366 дней.");
        await using var db = await factory.CreateDbContextAsync();
        var start = UtcStart(filter.From); var end = UtcStart(filter.To.AddDays(1));
        var q = db.AnalyticsVisits.AsNoTracking().Where(v => v.StartedAtUtc >= start && v.StartedAtUtc < end);
        if (filter.CityId == 0) q = q.Where(v => v.CityId == null);
        else if (filter.CityId.HasValue) q = q.Where(v => v.CityId == filter.CityId);
        if (!string.IsNullOrWhiteSpace(filter.Source)) q = q.Where(v => v.Source == filter.Source);
        if (!string.IsNullOrWhiteSpace(filter.Campaign)) q = q.Where(v => v.Campaign.Contains(filter.Campaign));
        if (!string.IsNullOrWhiteSpace(filter.Term)) q = q.Where(v => v.Term.Contains(filter.Term));
        if (!string.IsNullOrWhiteSpace(filter.Device)) q = q.Where(v => v.Device == filter.Device);
        var visits = await q.OrderBy(v => v.StartedAtUtc).Take(50001).ToListAsync();
        if (visits.Count > 50000) throw new ArgumentException("В выборке больше 50 000 визитов. Сократите период или уточните фильтры.");
        var eventQ = from e in db.AnalyticsEvents.AsNoTracking()
                     join v in q on e.VisitId equals v.Id
                     where e.CreatedAtUtc >= start && e.CreatedAtUtc < end
                     select e;
        var events = await eventQ.GroupBy(e => new { e.VisitId, e.Kind, e.Location, e.Path })
            .Select(g => new EventGroup { VisitId = g.Key.VisitId, Kind = g.Key.Kind,
                Location = g.Key.Location, Path = g.Key.Path, Count = g.Count() }).ToListAsync();
        var requests = await (from r in db.Requests.AsNoTracking()
                              join v in q on r.AnalyticsVisitId equals (Guid?)v.Id
                              where !r.IsManual && r.DateCreate >= start.ToLocalTime() && r.DateCreate < end.ToLocalTime()
                              group r by v.Id into g
                              select new { Id = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.Count);
        var hosts = await eventQ.Where(e => e.DailyHost != null)
            .GroupBy(e => e.CreatedAtUtc.AddHours(3).Date)
            .Select(g => new { Day = g.Key, Count = g.Select(e => e.DailyHost).Distinct().Count() }).ToListAsync();
        var eventMap = events.ToLookup(e => e.VisitId);
        TrafficNumbers Sum(IEnumerable<AnalyticsVisit> list)
        {
            var items = list.ToList(); var n = new TrafficNumbers { Visits = items.Count, Visitors = items.Select(v => v.VisitorId).Distinct().Count() };
            foreach (var v in items)
            {
                var ev = eventMap[v.Id].ToList();
                int Count(string k) => ev.Where(e => e.Kind == k).Sum(e => e.Count);
                var calc = Count("calculator"); var calls = Count("phone_click");
                var req = requests.GetValueOrDefault(v.Id);
                n.Views += Count("page_view"); n.Calculations += calc; n.CalcVisits += calc > 0 ? 1 : 0;
                n.Calls += calls; n.CallVisits += calls > 0 ? 1 : 0;
                n.Starts += Count("form_start") > 0 ? 1 : 0;
                n.Attempts += Count("form_attempt"); n.Invalid += Count("form_invalid"); n.Errors += Count("form_error");
                n.Requests += req; n.RequestVisits += req > 0 ? 1 : 0;
                n.ContactVisits += req > 0 || calls > 0 ? 1 : 0;
                // Co-occurrence, not proof of event order or causation.
                n.CalcContacts += calc > 0 && (req > 0 || calls > 0) ? 1 : 0;
            }
            return n;
        }
        List<TrafficGroup> Groups(Func<AnalyticsVisit, string> key) => visits.GroupBy(key)
            .Select(g => new TrafficGroup(g.Key, Sum(g))).OrderByDescending(g => g.Numbers.Visits).ToList();
        List<TrafficCount> EventCounts(string kind, Func<EventGroup, string> key) => events.Where(e => e.Kind == kind)
            .GroupBy(key).Select(g => new TrafficCount(g.Key, g.Sum(e => e.Count))).OrderByDescending(g => g.Count).Take(30).ToList();
        var report = new TrafficReport {
            InstalledAtUtc = await db.AnalyticsStates.Where(s => s.Id == 1).Select(s => (DateTime?)s.InstalledAtUtc).FirstOrDefaultAsync(),
            Total = Sum(visits), Sources = Groups(v => SiteAnalytics.Title(v.Source)),
            SourceDays = visits.GroupBy(v => new { Day = v.StartedAtUtc.AddHours(3).Date, v.Source })
                .Select(g => new TrafficSourceDay(g.Key.Day, g.Key.Source, g.Count())).ToList(),
            Cities = Groups(v => v.CityName), Devices = Groups(v => DeviceTitle(v.Device)),
            Browsers = visits.GroupBy(v => v.Browser).Select(g => new TrafficCount(g.Key, g.Count())).OrderByDescending(g => g.Count).ToList(),
            Referrers = visits.Where(v => v.ReferrerHost.Length > 0).GroupBy(v => v.ReferrerHost)
                .Select(g => new TrafficCount(g.Key, g.Count())).OrderByDescending(g => g.Count).Take(20).ToList(),
            Pages = EventCounts("page_view", e => e.Path), Phones = EventCounts("phone_click", e => Place(e.Location)),
            Blocks = EventCounts("block_view", e => Place(e.Location))
        };
        // Keywords are reported for paid campaigns only.
        report.Campaigns = visits.Where(v => v.Source is "yandex_ads" or "google_ads" or "other_campaign")
            .GroupBy(v => SiteAnalytics.Title(v.Source) + " · " + Empty(v.Campaign) + " · " + Empty(v.Term) + " · объявление: " + Empty(v.Content))
            .Select(g => new TrafficGroup(g.Key, Sum(g))).OrderByDescending(g => g.Numbers.Visits).ToList();
        var weightGroups = await eventQ.Where(e => e.Kind == "calculator" && e.Weight.HasValue)
            .GroupBy(e => e.Weight <= 50 ? "До 50 кг" : e.Weight <= 100 ? "50–100 кг" : e.Weight <= 300 ? "100–300 кг" : "Более 300 кг")
            .Select(g => new { Name = g.Key, Count = g.Count() }).ToListAsync();
        report.Weights = weightGroups.Select(x => new TrafficCount(x.Name, x.Count)).OrderByDescending(x => x.Count).ToList();
        var daily = visits.GroupBy(v => v.StartedAtUtc.AddHours(3).Date).ToDictionary(g => g.Key, g => Sum(g));
        for (var day = filter.From.Date; day <= filter.To.Date; day = day.AddDays(1))
            report.Days.Add(new(day, daily.GetValueOrDefault(day) ?? new(), hosts.FirstOrDefault(h => h.Day == day)?.Count ?? 0));
        return report;
    }
    private static string Empty(string x) => string.IsNullOrWhiteSpace(x) ? "Не передано" : x;
    public static string DeviceTitle(string x) => x switch { "mobile" => "Телефон", "tablet" => "Планшет", _ => "Компьютер" };
    public static string Place(string x) => x switch {
        "hero" => "Первый экран", "footer" => "Подвал", "navigation" => "Меню", "contacts" => "Контакты",
        "plan" => "Калькулятор / цены", "about" => "О нас", "services" => "Услуги",
        "team" => "Вес техники", "faqs" => "Частые вопросы", _ => x.Length > 0 ? x : "Не определено"
    };
}

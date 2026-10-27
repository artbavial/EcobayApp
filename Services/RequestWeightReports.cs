using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services;

public sealed record WeightCity(string Name, decimal Kg, int Completed, int Measured);
public sealed record WeightDay(DateTime Day, decimal Kg, int Completed, int Measured);
public sealed class RequestWeightReport
{
    public decimal TotalKg { get; init; }
    public int Completed { get; init; }
    public int Measured { get; init; }
    public int Missing => Completed - Measured;
    public decimal AverageKg => Measured == 0 ? 0 : TotalKg / Measured;
    public List<WeightCity> Cities { get; init; } = new();
    public List<WeightDay> Days { get; init; } = new();
}
public sealed class RequestWeightReports(IDbContextFactory<AppDBContext> factory, SitePermissions permissions)
{
    private sealed record WeightRow(DateTime CompletedAt, decimal? Kg, string City);
    public async Task<RequestWeightReport> LoadAsync(DateTime from, DateTime to, long? cityId, string source)
    {
        await permissions.RequireStaffAsync();
        if (to.Date < from.Date || (to.Date - from.Date).Days > 365)
            throw new ArgumentException("Выберите период не более 366 дней.");
        var start = AnalyticsReports.UtcStart(from).ToLocalTime();
        var end = AnalyticsReports.UtcStart(to.AddDays(1)).ToLocalTime();
        await using var db = await factory.CreateDbContextAsync();
        var q = db.Requests.AsNoTracking().Where(r => r.Status == RequestStatus.Done &&
            r.ProcessedAt.HasValue && r.ProcessedAt >= start && r.ProcessedAt < end);
        if (source == "manual") q = q.Where(r => r.IsManual);
        else if (source == "site") q = q.Where(r => !r.IsManual);
        var projected = AdminReporting.Rows(q);
        if (cityId == 0) projected = projected.Where(r => r.CityId == null);
        else if (cityId.HasValue) projected = projected.Where(r => r.CityId == cityId);
        var rows = await projected.OrderBy(r => r.Id).Select(r => new WeightRow(r.ProcessedAt!.Value, r.WeightKg, r.City))
            .Take(50001).ToListAsync();
        if (rows.Count > 50000) throw new ArgumentException("В выборке больше 50 000 отработанных заявок. Сократите период.");
        return Summarize(rows.Select(r => (Day: r.CompletedAt.ToUniversalTime().AddHours(3).Date, r.Kg, r.City)), from, to);
    }
    // Pure aggregation also used by small checks; unknown historical weights are never replaced with zero.
    public static RequestWeightReport Summarize(IEnumerable<(DateTime Day, decimal? Kg, string City)> input, DateTime from, DateTime to)
    {
        var rows = input.ToList();
        var daily = rows.GroupBy(r => r.Day.Date).ToDictionary(g => g.Key,
            g => new WeightDay(g.Key, g.Sum(r => r.Kg ?? 0), g.Count(), g.Count(r => r.Kg.HasValue)));
        var days = new List<WeightDay>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            days.Add(daily.GetValueOrDefault(d) ?? new(d, 0, 0, 0));
        return new RequestWeightReport {
            TotalKg = rows.Sum(r => r.Kg ?? 0), Completed = rows.Count, Measured = rows.Count(r => r.Kg.HasValue),
            Cities = rows.GroupBy(r => r.City).Select(g => new WeightCity(g.Key, g.Sum(r => r.Kg ?? 0), g.Count(), g.Count(r => r.Kg.HasValue)))
                .OrderByDescending(r => r.Kg).ThenBy(r => r.Name).ToList(), Days = days
        };
    }
}

using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;
namespace EcobayApp.Contex;

public static class AnalyticsSchema
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalyticsVisit>().HasIndex(x => new { x.StartedAtUtc, x.CityId });
        modelBuilder.Entity<AnalyticsEvent>().HasIndex(x => new { x.VisitId, x.CreatedAtUtc });
        modelBuilder.Entity<AnalyticsEvent>().HasIndex(x => new { x.CreatedAtUtc, x.Kind });
        modelBuilder.Entity<AnalyticsEvent>().Property(x => x.Weight).HasPrecision(12, 2);
        modelBuilder.Entity<AnalyticsEvent>().Property(x => x.AmountFrom).HasPrecision(12, 2);
        modelBuilder.Entity<AnalyticsEvent>().Property(x => x.AmountTo).HasPrecision(12, 2);
        modelBuilder.Entity<Request>().HasIndex(x => x.AnalyticsVisitId);
    }
}

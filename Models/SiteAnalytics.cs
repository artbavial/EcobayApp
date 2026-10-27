using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models;

public sealed class AnalyticsState
{
    [Key] public int Id { get; set; }
    public DateTime InstalledAtUtc { get; set; }
}

public sealed class AnalyticsVisit
{
    [Key] public Guid Id { get; set; }
    public Guid VisitorId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public long? CityId { get; set; }
    [MaxLength(120)] public string CityName { get; set; } = "";
    [MaxLength(300)] public string LandingPath { get; set; } = "/";
    [MaxLength(40)] public string Source { get; set; } = "direct";
    [MaxLength(200)] public string Campaign { get; set; } = "";
    [MaxLength(200)] public string Content { get; set; } = "";
    [MaxLength(200)] public string Term { get; set; } = "";
    [MaxLength(253)] public string ReferrerHost { get; set; } = "";
    [MaxLength(16)] public string Device { get; set; } = "";
    [MaxLength(24)] public string Browser { get; set; } = "";
}

public sealed class AnalyticsEvent
{
    [Key] public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    [MaxLength(32)] public string Kind { get; set; } = "";
    [MaxLength(300)] public string Path { get; set; } = "/";
    public long? CityId { get; set; }
    // A day-scoped HMAC. Never stores an IP address; not comparable across days.
    [MaxLength(64)] public string? DailyHost { get; set; }
    [MaxLength(40)] public string Location { get; set; } = "";
    public decimal? Weight { get; set; }
    public decimal? AmountFrom { get; set; }
    public decimal? AmountTo { get; set; }
}

public sealed record SiteEventInput(
    Guid Id, Guid VisitId, Guid VisitorId, string? Kind, string? Path,
    string? UtmSource, string? UtmMedium, string? Campaign, string? Content, string? Term,
    string? Referrer, bool Yclid, bool Gclid, string? Location,
    decimal? Weight, decimal? AmountFrom, decimal? AmountTo);

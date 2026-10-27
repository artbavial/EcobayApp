using System.ComponentModel.DataAnnotations;
namespace EcobayApp.Models;

public sealed class RequestSubmission
{
    [Key] public Guid Id { get; set; }
    public long RequestId { get; set; }
    [Required, MaxLength(64)] public string Fingerprint { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}
public sealed class RequestEmailOutbox
{
    public long Id { get; set; }
    public long RequestId { get; set; }
    [Required] public string PayloadJson { get; set; } = "";
    public int Attempts { get; set; }
    public DateTime AvailableAtUtc { get; set; }
    public DateTime? LeaseUntilUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    [MaxLength(1000)] public string? LastError { get; set; }
}
public sealed class PhoneClick
{
    [Key] public Guid Id { get; set; }
    public long? CityId { get; set; }
    [MaxLength(120)] public string CityName { get; set; } = "Общая страница";
    [MaxLength(300)] public string Path { get; set; } = "/";
    public DateTime CreatedAtUtc { get; set; }
}

public sealed record PhoneClickInput(Guid Id, string? Path);

using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models;

/// <summary>An operator's mark, not a measured phone conversation or request status.</summary>
public sealed class RequestContactEvent
{
    [Key] public long Id { get; set; }
    public long RequestId { get; set; }
    public bool IsCalled { get; set; }
    public DateTime CallAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public long UserId { get; set; }
    [MaxLength(100)] public string UserName { get; set; } = "";
}

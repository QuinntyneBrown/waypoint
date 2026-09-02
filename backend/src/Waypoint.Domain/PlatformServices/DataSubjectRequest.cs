namespace Waypoint.Domain.PlatformServices;

public enum DataSubjectRequestStatus
{
    Pending = 1,
    Completed = 2,
}

/// <summary>Durable workflow record for an authenticated data-subject right request.</summary>
public sealed class DataSubjectRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Reference { get; set; } = string.Empty;
    public Guid SubjectUserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? RequestedDisplayName { get; set; }
    public string? RequestedEmail { get; set; }
    public string? RequestedPronouns { get; set; }
    public bool RequestedClearPronouns { get; set; }
    public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    public DataSubjectRequestStatus Status { get; set; } = DataSubjectRequestStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public Guid? CompletedByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ResolutionNote { get; set; }
}

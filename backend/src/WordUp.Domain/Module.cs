namespace WordUp.Domain;

/// <summary>A module within a track (L2-CURRIC-001).</summary>
public class Module
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TrackId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public Guid? PrerequisiteModuleId { get; set; }
    public string? ScriptureAnchor { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

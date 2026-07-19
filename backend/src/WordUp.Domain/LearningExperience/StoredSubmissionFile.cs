namespace WordUp.Domain.LearningExperience;

public enum SubmissionFileKind
{
    Photo = 0,
    File = 1,
}

/// <summary>Private, validated assignment evidence stored as non-executable database content.</summary>
public sealed class StoredSubmissionFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public Guid AssignmentId { get; set; }
    public SubmissionFileKind Kind { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

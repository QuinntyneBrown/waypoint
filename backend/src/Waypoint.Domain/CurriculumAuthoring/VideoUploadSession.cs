namespace Waypoint.Domain.CurriculumAuthoring;

/// <summary>One in-progress chunked lesson-video upload.</summary>
public sealed class VideoUploadSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long ExpectedBytes { get; set; }
    public int ChunkSizeBytes { get; set; }
    public int ChunkCount { get; set; }
    /// <summary>One bit per chunk index; a set bit means the chunk has been written.</summary>
    public byte[] ReceivedChunks { get; set; } = [];
    /// <summary>Guards concurrent chunk requests that update the received-chunk bits.</summary>
    public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}

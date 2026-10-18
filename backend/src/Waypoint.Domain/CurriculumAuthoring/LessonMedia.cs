namespace Waypoint.Domain.CurriculumAuthoring;

public class LessonMedia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public string MediaType { get; set; } = string.Empty; // "video" | "document"
    public string Url { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public byte[]? Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Self-hosted streaming pipeline (upload-lesson-video design). A streamed video keeps no
    // bytes in Content and has an empty Url; it is served through a signed playback URL.
    public VideoStatus VideoStatus { get; set; } = VideoStatus.None;
    public double? DurationSeconds { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int ProgressPercent { get; set; }
    public string? FailureCode { get; set; }
}

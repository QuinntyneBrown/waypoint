namespace WordUp.Domain.CurriculumAuthoring;

public class LessonMedia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public string MediaType { get; set; } = string.Empty; // "video" | "document"
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

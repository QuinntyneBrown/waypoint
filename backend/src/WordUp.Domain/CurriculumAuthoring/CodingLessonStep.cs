namespace WordUp.Domain.CurriculumAuthoring;

/// <summary>An authored, ordered step and self-check criterion for an interactive coding lesson.</summary>
public class CodingLessonStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string CodeSample { get; set; } = string.Empty;
    public string ChecklistLabel { get; set; } = string.Empty;
}

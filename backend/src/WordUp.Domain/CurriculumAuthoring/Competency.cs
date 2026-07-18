namespace WordUp.Domain.CurriculumAuthoring;

/// <summary>A named competency within a progression stage (L2-CURRIC-006).</summary>
public class Competency
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

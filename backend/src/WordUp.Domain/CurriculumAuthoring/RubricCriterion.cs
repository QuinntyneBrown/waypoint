namespace WordUp.Domain.CurriculumAuthoring;

public class RubricCriterion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssignmentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Order { get; set; }
}

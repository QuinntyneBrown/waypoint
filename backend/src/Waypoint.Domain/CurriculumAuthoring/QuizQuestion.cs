namespace Waypoint.Domain.CurriculumAuthoring;

public class QuizQuestion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuizId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
    public int Order { get; set; }
    public string? OptionsJson { get; set; }        // JSON array of options for choice types
    public string? CorrectAnswersJson { get; set; } // JSON array of correct option ids / answer(s)
}

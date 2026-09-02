using FluentValidation;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed class AddQuizQuestionCommandValidator : AbstractValidator<AddQuizQuestionCommand>
{
    public AddQuizQuestionCommandValidator()
    {
        RuleFor(item => item.Text).NotEmpty().MaximumLength(1000);
        RuleFor(item => item.Type).IsInEnum();
        RuleFor(item => item)
            .Must(IsValidForType)
            .WithMessage("Question options and correct answers must match the selected question type.");
    }

    private static bool IsValidForType(AddQuizQuestionCommand item)
    {
        var options = item.Options ?? [];
        var indexes = item.CorrectIndexes?.Distinct().ToList() ?? [];
        var answers = item.CorrectAnswers?
            .Where(answer => !string.IsNullOrWhiteSpace(answer))
            .Select(answer => answer.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
        var indexesInRange = indexes.All(index => index >= 0 && index < options.Count);
        return item.Type switch
        {
            QuestionType.MultipleChoice =>
                options.Count >= 2 && indexes.Count == 1 && indexesInRange && answers.Count == 0,
            QuestionType.TrueFalse =>
                options.Count == 2 && indexes.Count == 1 && indexesInRange && answers.Count == 0,
            QuestionType.MultipleAnswer =>
                options.Count >= 2 && indexes.Count >= 1 && indexesInRange && answers.Count == 0,
            QuestionType.ShortAnswer =>
                options.Count == 0 && indexes.Count == 0 && answers.Count >= 1,
            _ => false,
        };
    }
}

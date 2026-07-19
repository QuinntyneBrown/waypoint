using FluentValidation;

namespace WordUp.Application.CurriculumAuthoring.Assessments;

public sealed class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
{
    private static readonly string[] FeedbackModes =
        ["show-correct", "score-only", "after-all-attempts"];

    public CreateQuizCommandValidator()
    {
        RuleFor(item => item.PassMark).InclusiveBetween(1, 100);
        RuleFor(item => item.AttemptsAllowed).InclusiveBetween(1, 10);
        RuleFor(item => item.FeedbackMode)
            .Must(mode => FeedbackModes.Contains(mode, StringComparer.Ordinal))
            .WithMessage("Feedback mode must be show-correct, score-only, or after-all-attempts.");
    }
}

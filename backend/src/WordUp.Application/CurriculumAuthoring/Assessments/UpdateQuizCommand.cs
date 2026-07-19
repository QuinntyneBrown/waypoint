using FluentValidation;
using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Assessments;

public sealed record UpdateQuizCommand(
    Guid QuizId,
    Guid? ModuleId,
    Guid? LessonId,
    int PassMark,
    int AttemptsAllowed,
    string FeedbackMode,
    bool Shuffle,
    bool RequiredToCompleteModule) : IRequest;

public sealed class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        RuleFor(item => item.QuizId).NotEmpty();
        RuleFor(item => item.PassMark).InclusiveBetween(1, 100);
        RuleFor(item => item.AttemptsAllowed).InclusiveBetween(1, 10);
        RuleFor(item => item.FeedbackMode)
            .Must(mode => mode is "show-correct" or "score-only" or "after-all-attempts")
            .WithMessage("Feedback mode must be show-correct, score-only, or after-all-attempts.");
        RuleFor(item => item)
            .Must(item => item.ModuleId is null || item.LessonId is null)
            .WithMessage("A quiz may be attached to a module or lesson, not both.");
    }
}

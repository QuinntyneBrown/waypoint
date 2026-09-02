using FluentValidation;

namespace Waypoint.Application.Mentoring.QuizApproval;

public sealed class ApproveQuizAttemptCommandValidator : AbstractValidator<ApproveQuizAttemptCommand>
{
    public ApproveQuizAttemptCommandValidator()
    {
        RuleFor(command => command.OverrideScore)
            .InclusiveBetween(0, 100)
            .When(command => command.OverrideScore.HasValue)
            .WithMessage("Override score must be between 0 and 100.");
    }
}

using FluentValidation;

namespace Waypoint.Application.ApplicationIntake;

public sealed class ApproveApplicationCommandValidator : AbstractValidator<ApproveApplicationCommand>
{
    public ApproveApplicationCommandValidator()
    {
        RuleFor(command => command.ApplicationId).NotEmpty();
        RuleFor(command => command.CohortId).NotEmpty();
        RuleFor(command => command.MentorId).NotEmpty();
        RuleFor(command => command.TrackInterest).MaximumLength(200);
        RuleFor(command => command.DecisionNote).MaximumLength(2000);
    }
}

using FluentValidation;

namespace Waypoint.Application.Engagement.Messaging;

public sealed class BroadcastToGroupCommandValidator : AbstractValidator<BroadcastToGroupCommand>
{
    public BroadcastToGroupCommandValidator()
    {
        RuleFor(command => command.CohortId).NotEmpty();
        RuleFor(command => command.Body)
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .WithMessage("Message body is required.")
            .MaximumLength(4000);
    }
}

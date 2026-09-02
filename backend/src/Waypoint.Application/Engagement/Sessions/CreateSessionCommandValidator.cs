using FluentValidation;

namespace Waypoint.Application.Engagement.Sessions;

public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CohortId).NotEmpty();
        RuleFor(x => x.MentorId).NotEmpty().WithMessage("A mentor is required.");
        RuleFor(x => x.StartsAt).NotEmpty();
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 1440)
            .When(x => x.DurationMinutes is not null);
        RuleFor(x => x.Location).MaximumLength(256);
    }
}

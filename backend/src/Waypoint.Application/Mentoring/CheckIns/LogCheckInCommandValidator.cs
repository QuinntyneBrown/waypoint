using FluentValidation;

namespace Waypoint.Application.Mentoring.CheckIns;

public class LogCheckInCommandValidator : AbstractValidator<LogCheckInCommand>
{
    private static readonly string[] AllowedTypes = { "1:1", "call", "session" };

    public LogCheckInCommandValidator()
    {
        RuleFor(x => x.YouthId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Type must be one of: 1:1, call, session.");
        RuleFor(x => x.Engagement).MaximumLength(2000);
        RuleFor(x => x.GoalsDiscussed).MaximumLength(2000);
        RuleFor(x => x.PrayerRequests).MaximumLength(2000);
        RuleFor(x => x.ActionItems).MaximumLength(2000);
    }
}

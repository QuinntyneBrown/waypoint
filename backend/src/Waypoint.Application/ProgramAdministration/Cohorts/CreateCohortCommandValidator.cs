using FluentValidation;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

public class CreateCohortCommandValidator : AbstractValidator<CreateCohortCommand>
{
    public CreateCohortCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.LeadMentorId).NotEmpty();
        RuleFor(x => x.Location).MaximumLength(256);
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date.");
    }
}

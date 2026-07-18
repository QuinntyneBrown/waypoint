using FluentValidation;

namespace WordUp.Application.Cohorts;

public class CreateCohortCommandValidator : AbstractValidator<CreateCohortCommand>
{
    public CreateCohortCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.LeadMentorId).NotEmpty();
    }
}

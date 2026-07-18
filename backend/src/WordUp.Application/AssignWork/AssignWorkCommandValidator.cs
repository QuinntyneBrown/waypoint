using FluentValidation;

namespace WordUp.Application.AssignWork;

public class AssignWorkCommandValidator : AbstractValidator<AssignWorkCommand>
{
    public AssignWorkCommandValidator()
    {
        RuleFor(x => x.ItemType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("An item to assign is required.");
        RuleFor(x => x)
            .Must(HaveExactlyOneTarget)
            .WithMessage("Provide exactly one of youthIds (non-empty) or cohortId.");
    }

    private static bool HaveExactlyOneTarget(AssignWorkCommand command)
    {
        var hasYouth = command.YouthIds is { Count: > 0 };
        var hasCohort = command.CohortId is not null;
        return hasYouth ^ hasCohort;
    }
}

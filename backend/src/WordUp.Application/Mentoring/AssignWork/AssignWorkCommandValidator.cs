using FluentValidation;

namespace WordUp.Application.Mentoring.AssignWork;

public class AssignWorkCommandValidator : AbstractValidator<AssignWorkCommand>
{
    public AssignWorkCommandValidator()
    {
        RuleFor(x => x.ItemType)
            .NotEmpty()
            .Must(type => AssignWorkItemTypes.All.Contains(type))
            .WithMessage("Item type must be Lesson, Module, or Project.");
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("An item to assign is required.");
        RuleFor(x => x)
            .Must(HaveExactlyOneTarget)
            .WithMessage("Provide exactly one of youthIds (non-empty) or cohortId.");
        RuleFor(x => x.YouthIds)
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
            .WithMessage("Youth ids must be valid.");
        RuleFor(x => x.ReminderCadence)
            .Must(cadence => string.IsNullOrWhiteSpace(cadence)
                || ReminderCadences.All.Contains(cadence))
            .WithMessage("Reminder cadence must be 1-day-before or 2-days-before.");
        RuleFor(x => x)
            .Must(command => string.IsNullOrWhiteSpace(command.ReminderCadence)
                || command.DueDate is not null)
            .WithMessage("A due date is required when a reminder is set.");
        RuleFor(x => x.Message).MaximumLength(4000);
    }

    private static bool HaveExactlyOneTarget(AssignWorkCommand command)
    {
        var hasYouth = command.YouthIds is { Count: > 0 };
        var hasCohort = command.CohortId is not null;
        return hasYouth ^ hasCohort;
    }
}

using FluentValidation;

namespace WordUp.Application.Assessments;

public class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TaskDescription).NotEmpty();
        RuleFor(x => x.SubmissionTypes)
            .NotNull()
            .Must(types => types is not null && types.Count >= 1)
            .WithMessage("An assignment must accept at least one submission type.");
    }
}

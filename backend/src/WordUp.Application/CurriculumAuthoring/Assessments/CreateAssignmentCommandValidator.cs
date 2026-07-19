using FluentValidation;

namespace WordUp.Application.CurriculumAuthoring.Assessments;

public class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    private static readonly HashSet<string> AllowedSubmissionTypes =
        new(["text", "photo", "link", "file"], StringComparer.OrdinalIgnoreCase);

    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TaskDescription).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.SubmissionTypes)
            .NotNull()
            .Must(types => types is not null && types.Count >= 1)
            .WithMessage("An assignment must accept at least one submission type.")
            .Must(types => types is not null
                && types.All(type => AllowedSubmissionTypes.Contains(type.Trim())))
            .WithMessage("Submission types must be text, photo, link, or file.");
        RuleFor(x => x.GracePeriodDays).InclusiveBetween(0, 365);
        RuleForEach(x => x.Rubric).ChildRules(criterion =>
        {
            criterion.RuleFor(item => item.Description).NotEmpty().MaximumLength(500);
            criterion.RuleFor(item => item.Points).InclusiveBetween(1, 1000);
        });
        RuleFor(x => x.Rubric)
            .Must(rubric => rubric is null || rubric.Sum(item => item.Points) <= 10000)
            .WithMessage("The rubric total cannot exceed 10,000 points.");
    }
}

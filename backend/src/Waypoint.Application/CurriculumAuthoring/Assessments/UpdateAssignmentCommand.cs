using FluentValidation;
using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed record UpdateAssignmentCommand(
    Guid AssignmentId,
    Guid? ModuleId,
    Guid? LessonId,
    string Title,
    string TaskDescription,
    IReadOnlyList<string> SubmissionTypes,
    DateTime? DueDate,
    int GracePeriodDays,
    bool AllowLate,
    bool RequireMentorApproval,
    IReadOnlyList<RubricInput> Rubric) : IRequest;

public sealed class UpdateAssignmentCommandValidator : AbstractValidator<UpdateAssignmentCommand>
{
    private static readonly HashSet<string> AllowedSubmissionTypes =
        new(["text", "photo", "link", "file"], StringComparer.OrdinalIgnoreCase);

    public UpdateAssignmentCommandValidator()
    {
        RuleFor(item => item.AssignmentId).NotEmpty();
        RuleFor(item => item.Title).NotEmpty().MaximumLength(200);
        RuleFor(item => item.TaskDescription).NotEmpty().MaximumLength(10000);
        RuleFor(item => item.SubmissionTypes)
            .NotNull()
            .Must(types => types is { Count: > 0 }
                && types.All(type => AllowedSubmissionTypes.Contains(type.Trim())))
            .WithMessage("Choose at least one of text, photo, link, or file.");
        RuleFor(item => item.GracePeriodDays).InclusiveBetween(0, 365);
        RuleForEach(item => item.Rubric).ChildRules(criterion =>
        {
            criterion.RuleFor(item => item.Description).NotEmpty().MaximumLength(500);
            criterion.RuleFor(item => item.Points).InclusiveBetween(1, 1000);
        });
        RuleFor(item => item.Rubric)
            .Must(rubric => rubric is null || rubric.Sum(item => item.Points) <= 10000)
            .WithMessage("The rubric total cannot exceed 10,000 points.");
    }
}

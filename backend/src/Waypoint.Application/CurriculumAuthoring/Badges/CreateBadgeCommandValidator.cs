using FluentValidation;

namespace Waypoint.Application.CurriculumAuthoring.Badges;

public sealed class CreateBadgeCommandValidator : AbstractValidator<CreateBadgeCommand>
{
    private static readonly HashSet<string> AllowedAutoRules = new(StringComparer.OrdinalIgnoreCase)
    {
        "module-completed",
        "lesson-completed",
        "quiz-passed",
        "attendance-streak",
        "assignment-approved",
        "coding-checklist-completed",
    };

    private static readonly HashSet<string> RulesRequiringTarget = new(StringComparer.OrdinalIgnoreCase)
    {
        "module-completed",
        "lesson-completed",
        "quiz-passed",
        "assignment-approved",
        "coding-checklist-completed",
    };

    public CreateBadgeCommandValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(128);
        RuleFor(item => item.Icon).NotEmpty().MaximumLength(128);
        RuleFor(item => item.Description).MaximumLength(1000);
        RuleFor(item => item.CriteriaKind).IsInEnum();

        When(item => item.CriteriaKind == BadgeCriteriaKind.Manual, () =>
        {
            RuleFor(item => item.AutoRule)
                .Empty()
                .WithMessage("A manual badge cannot have an automatic rule.");
            RuleFor(item => item.TargetRefId)
                .Empty()
                .WithMessage("A manual badge cannot have an automatic-rule target.");
        });

        When(item => item.CriteriaKind == BadgeCriteriaKind.Auto, () =>
        {
            RuleFor(item => item.AutoRule)
                .NotEmpty()
                .Must(rule => rule is not null && AllowedAutoRules.Contains(rule.Trim()))
                .WithMessage("The automatic badge rule is not supported.");

            RuleFor(item => item.TargetRefId)
                .NotEmpty()
                .When(item => item.AutoRule is not null && RulesRequiringTarget.Contains(item.AutoRule.Trim()))
                .WithMessage("This automatic badge rule requires a target.");

            RuleFor(item => item.TargetRefId)
                .Empty()
                .When(item => string.Equals(item.AutoRule?.Trim(), "attendance-streak", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Attendance-streak badges do not accept a target.");
        });
    }
}

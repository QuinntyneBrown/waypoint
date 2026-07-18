using FluentValidation;

namespace WordUp.Application.Mentoring.Grading;

public class GradeSubmissionCommandValidator : AbstractValidator<GradeSubmissionCommand>
{
    private static readonly string[] ValidDecisions = { "approve", "request-revision", "save-draft" };

    public GradeSubmissionCommandValidator()
    {
        RuleFor(x => x.Decision)
            .Must(d => ValidDecisions.Contains(d))
            .WithMessage("Decision must be approve, request-revision, or save-draft.");

        // A revision request must tell the youth what to fix (L2-MENTOR-003 AC2).
        RuleFor(x => x.Comment)
            .NotEmpty()
            .When(x => x.Decision == "request-revision")
            .WithMessage("A comment is required when requesting revisions.");
    }
}

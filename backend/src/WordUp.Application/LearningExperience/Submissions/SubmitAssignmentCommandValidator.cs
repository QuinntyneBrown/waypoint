using FluentValidation;

namespace WordUp.Application.LearningExperience.Submissions;

public class SubmitAssignmentCommandValidator : AbstractValidator<SubmitAssignmentCommand>
{
    public SubmitAssignmentCommandValidator()
    {
        // Youth must attest before submitting (L2-LEARN-006 AC3).
        RuleFor(x => x.Attest)
            .Equal(true)
            .WithMessage("You must confirm this is your own work before submitting.");

        // At least one form of content must be present (L2-LEARN-006 AC1).
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.BodyText) ||
                x.PhotoUploadId is not null ||
                !string.IsNullOrWhiteSpace(x.PhotoUrl) ||
                !string.IsNullOrWhiteSpace(x.LinkUrl))
            .WithMessage("Provide at least one of a written response, a photo, or a link.");
    }
}

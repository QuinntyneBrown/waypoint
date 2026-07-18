using FluentValidation;

namespace WordUp.Application.LearningExperience.Reflections;

public class SaveReflectionCommandValidator : AbstractValidator<SaveReflectionCommand>
{
    public SaveReflectionCommandValidator()
    {
        // A submitted (non-draft) reflection must have text; drafts may be empty (L2-LEARN-003 AC3/AC4).
        RuleFor(x => x.Text)
            .NotEmpty()
            .When(x => !x.IsDraft)
            .WithMessage("Reflection text is required.");
    }
}

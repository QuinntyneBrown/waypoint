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
        RuleFor(x => x.Mood)
            .NotEmpty()
            .When(x => !x.IsDraft)
            .WithMessage("Choose how you are feeling.");
        RuleFor(x => x.Text).MaximumLength(8000);
        RuleFor(x => x.Mood).MaximumLength(50);
    }
}

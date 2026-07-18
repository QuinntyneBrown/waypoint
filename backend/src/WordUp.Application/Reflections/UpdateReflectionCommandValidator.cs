using FluentValidation;

namespace WordUp.Application.Reflections;

public class UpdateReflectionCommandValidator : AbstractValidator<UpdateReflectionCommand>
{
    public UpdateReflectionCommandValidator()
    {
        // A submitted (non-draft) reflection must have text; drafts may be empty (L2-LEARN-003 AC3/AC4).
        RuleFor(x => x.Text)
            .NotEmpty()
            .When(x => !x.IsDraft)
            .WithMessage("Reflection text is required.");
    }
}

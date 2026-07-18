using FluentValidation;

namespace WordUp.Application.PublicSite;

public class SubmitContactRequestCommandValidator : AbstractValidator<SubmitContactRequestCommand>
{
    public SubmitContactRequestCommandValidator()
    {
        // Required fields + malformed email are blocked with field-level errors (L2-PUB-003 AC1).
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Topic).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}

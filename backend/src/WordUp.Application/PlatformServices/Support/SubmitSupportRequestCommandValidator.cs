using FluentValidation;

namespace WordUp.Application.PlatformServices.Support;

public class SubmitSupportRequestCommandValidator : AbstractValidator<SubmitSupportRequestCommand>
{
    public SubmitSupportRequestCommandValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ReplyToEmail).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

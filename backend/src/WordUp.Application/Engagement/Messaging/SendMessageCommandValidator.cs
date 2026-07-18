using FluentValidation;

namespace WordUp.Application.Engagement.Messaging;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ToUserId).NotEmpty();
        RuleFor(x => x.Body)
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .WithMessage("Message body is required.")
            .MaximumLength(4000);
    }
}

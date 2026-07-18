using FluentValidation;

namespace WordUp.Application.Messaging;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ToUserId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
    }
}

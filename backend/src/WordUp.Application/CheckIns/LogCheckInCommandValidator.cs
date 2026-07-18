using FluentValidation;

namespace WordUp.Application.CheckIns;

public class LogCheckInCommandValidator : AbstractValidator<LogCheckInCommand>
{
    private static readonly string[] AllowedTypes = { "1:1", "call", "session" };

    public LogCheckInCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Type must be one of: 1:1, call, session.");
    }
}

using FluentValidation;

namespace WordUp.Application.Rsvps;

public class SubmitRsvpCommandValidator : AbstractValidator<SubmitRsvpCommand>
{
    public SubmitRsvpCommandValidator()
    {
        RuleFor(x => x.Response)
            .NotEmpty()
            .Must(r => SubmitRsvpCommand.TryParseResponse(r, out _))
            .WithMessage("Response must be one of: going, maybe, cant.");
    }
}

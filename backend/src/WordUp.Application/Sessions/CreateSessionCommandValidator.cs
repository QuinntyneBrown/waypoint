using FluentValidation;

namespace WordUp.Application.Sessions;

public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.CohortId).NotEmpty();
    }
}

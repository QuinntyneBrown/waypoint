using FluentValidation;

namespace WordUp.Application.ProgramAdministration.People;

public class ChangeRoleCommandValidator : AbstractValidator<ChangeRoleCommand>
{
    public ChangeRoleCommandValidator()
    {
        RuleFor(x => x.Role)
            .Must(Roles.IsValid)
            .WithMessage("Unknown role.");
    }
}

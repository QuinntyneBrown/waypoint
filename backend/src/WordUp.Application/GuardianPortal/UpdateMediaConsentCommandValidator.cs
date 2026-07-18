using FluentValidation;

namespace WordUp.Application.GuardianPortal;

public class UpdateMediaConsentCommandValidator : AbstractValidator<UpdateMediaConsentCommand>
{
    public UpdateMediaConsentCommandValidator()
    {
        RuleFor(x => x.ChildId).NotEmpty();
    }
}

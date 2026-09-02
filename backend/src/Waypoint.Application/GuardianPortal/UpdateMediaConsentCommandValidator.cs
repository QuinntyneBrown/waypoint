using FluentValidation;

namespace Waypoint.Application.GuardianPortal;

public class UpdateMediaConsentCommandValidator : AbstractValidator<UpdateMediaConsentCommand>
{
    public UpdateMediaConsentCommandValidator()
    {
        RuleFor(x => x.ChildId).NotEmpty();
    }
}

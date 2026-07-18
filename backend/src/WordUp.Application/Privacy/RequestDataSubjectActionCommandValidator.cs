using FluentValidation;

namespace WordUp.Application.Privacy;

public class RequestDataSubjectActionCommandValidator : AbstractValidator<RequestDataSubjectActionCommand>
{
    private static readonly string[] AllowedActionTypes = { "access", "correction", "removal", "consent-withdrawal" };

    public RequestDataSubjectActionCommandValidator()
    {
        RuleFor(x => x.ActionType)
            .NotEmpty()
            .Must(a => AllowedActionTypes.Any(x => string.Equals(x, a, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("ActionType must be one of: access, correction, removal, consent-withdrawal.");

        RuleFor(x => x.Details).MaximumLength(4000);
    }
}

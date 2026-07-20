using FluentValidation;

namespace WordUp.Application.PlatformServices.Privacy;

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
        RuleFor(x => x.Correction!.DisplayName).MaximumLength(128)
            .When(x => x.Correction is not null);
        RuleFor(x => x.Correction!.Email).EmailAddress().MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.Correction?.Email));
        RuleFor(x => x.Correction!.Pronouns).MaximumLength(64)
            .When(x => x.Correction is not null);
        RuleFor(x => x.Correction).Must(correction =>
                correction is null
                || !correction.ClearPronouns
                || string.IsNullOrWhiteSpace(correction.Pronouns))
            .WithMessage("Pronouns cannot be supplied when ClearPronouns is true.");
        RuleFor(x => x).Must(command =>
            !string.Equals(command.ActionType, "correction", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(command.Details)
            || command.Correction is not null
                && (!string.IsNullOrWhiteSpace(command.Correction.DisplayName)
                    || !string.IsNullOrWhiteSpace(command.Correction.Email)
                    || command.Correction.Pronouns is not null
                    || command.Correction.ClearPronouns))
            .WithMessage("A correction request must include at least one typed profile field.");
        RuleFor(x => x.Correction).Null()
            .When(x => !string.Equals(x.ActionType, "correction", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Correction fields are only accepted for correction requests.");
    }
}

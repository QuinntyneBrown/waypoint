using FluentValidation;

namespace WordUp.Application.Intake;

public class SubmitApplicationCommandValidator : AbstractValidator<SubmitApplicationCommand>
{
    public SubmitApplicationCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Neighbourhood).NotEmpty().MaximumLength(256);
        RuleFor(x => x.WhyInterested).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TrackInterest).MaximumLength(256);

        // Consent + privacy acknowledgements are mandatory (L2-INTAKE-001 AC1).
        RuleFor(x => x.ParticipationConsentAck).Equal(true)
            .WithMessage("You must acknowledge participation consent to apply.");
        RuleFor(x => x.PrivacyAck).Equal(true)
            .WithMessage("You must acknowledge the privacy notice to apply.");

        // Eligible age range (L2-INTAKE-001 AC3).
        RuleFor(x => x.Age).InclusiveBetween(12, 18)
            .WithMessage("Applicants must be aged 12–18 inclusive.");

        // Minors must supply guardian contact details (L2-INTAKE-001 AC2).
        When(x => x.Age < 18, () =>
        {
            RuleFor(x => x.GuardianName).NotEmpty().MaximumLength(256)
                .WithMessage("A guardian name is required for applicants under 18.");
            RuleFor(x => x.GuardianEmail).NotEmpty().EmailAddress().MaximumLength(256)
                .WithMessage("A valid guardian email is required for applicants under 18.");
            RuleFor(x => x.GuardianPhone).NotEmpty().MaximumLength(64)
                .WithMessage("A guardian phone number is required for applicants under 18.");
        });
    }
}

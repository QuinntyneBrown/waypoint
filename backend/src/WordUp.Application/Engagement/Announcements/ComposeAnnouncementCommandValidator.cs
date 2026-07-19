using FluentValidation;

namespace WordUp.Application.Engagement.Announcements;

public class ComposeAnnouncementCommandValidator : AbstractValidator<ComposeAnnouncementCommand>
{
    private static readonly string[] AllowedScopes = { "org", "cohort" };
    private static readonly string[] AllowedTargeting =
        { "Youth", "YouthGuardians", "Guardians", "Mentors", "Everyone" };

    public ComposeAnnouncementCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);

        RuleFor(x => x.AudienceScope)
            .Must(s => AllowedScopes.Contains(s))
            .WithMessage("AudienceScope must be one of: org, cohort.");

        RuleFor(x => x.RoleTargeting)
            .Must(r => AllowedTargeting.Contains(r))
            .WithMessage("RoleTargeting must be one of: Youth, YouthGuardians, Guardians, Mentors, Everyone.");

        RuleFor(x => x.CohortId)
            .NotNull()
            .When(x => x.AudienceScope == "cohort")
            .WithMessage("CohortId is required for a cohort announcement.");
        RuleFor(x => x.CohortId)
            .Null()
            .When(x => x.AudienceScope == "org")
            .WithMessage("CohortId must be empty for an organization announcement.");
    }
}

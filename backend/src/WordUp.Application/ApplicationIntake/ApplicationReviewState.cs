namespace WordUp.Application.ApplicationIntake;

internal static class ApplicationReviewState
{
    public static string ConsentState(YouthApplication application, bool hasValidConsent) =>
        !application.IsMinor
            ? "Not required"
            : hasValidConsent
                ? "Recorded"
                : "Needs consent";

    public static string EffectiveStatus(YouthApplication application, bool hasValidConsent)
    {
        if (application.Status == ApplicationStatus.Approved)
            return "Approved";
        if (application.Status == ApplicationStatus.Declined)
            return "Declined";
        if (application.IsMinor && !hasValidConsent)
            return "Needs consent";
        return application.Status == ApplicationStatus.New ? "New" : "In review";
    }

    public static bool CanApprove(YouthApplication application, bool hasValidConsent) =>
        application.Status is not ApplicationStatus.Approved and not ApplicationStatus.Declined
        && (!application.IsMinor || hasValidConsent);
}

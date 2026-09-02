namespace Waypoint.Domain.ApplicationIntake;

public enum ApplicationStatus
{
    New = 1,
    InReview = 2,
    NeedsConsent = 3,
    Approved = 4,
    Declined = 5,
}

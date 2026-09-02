namespace Waypoint.Api.Contracts.ApplicationIntake;

public record SubmitApplicationRequest(
    string FirstName,
    string LastName,
    int Age,
    string WhoIsFilling,
    string Neighbourhood,
    string CohortInterest,
    string WhyInterested,
    string? TrackInterest,
    string? GuardianName,
    string? GuardianEmail,
    string? GuardianPhone,
    bool ParticipationConsentAck,
    bool PrivacyAck);

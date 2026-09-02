namespace Waypoint.Api.Contracts.PublicSite;

public record VolunteerSignupBody(
    string Name,
    string Email,
    string? Phone,
    string? AreasOfInterest,
    bool ScreeningConsentAck);

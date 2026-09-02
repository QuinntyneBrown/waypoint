namespace Waypoint.Api.Contracts.GuardianPortal;

public record UpdateMediaConsentRequest(
    bool MediaConsent,
    bool MediaAllPhotosVideo,
    bool MediaGroupOnly,
    bool MediaFirstNameCaptions);

namespace Waypoint.Api.Contracts.ConsentAndSafeguarding;

public record CaptureConsentRequest(
    Guid YouthUserId,
    string GuardianName,
    string Relationship,
    bool ParticipationConsent,
    bool MediaConsent,
    bool MediaAllPhotosVideo,
    bool MediaGroupOnly,
    bool MediaFirstNameCaptions,
    string Signature,
    string? Term);

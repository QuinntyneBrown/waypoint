namespace WordUp.Api.Contracts.ConsentAndSafeguarding;

public sealed record CaptureRequestedConsentRequest(
    string GuardianName,
    string Relationship,
    bool ParticipationConsent,
    bool MediaConsent,
    bool MediaAllPhotosVideo,
    bool MediaGroupOnly,
    bool MediaFirstNameCaptions,
    string Signature);

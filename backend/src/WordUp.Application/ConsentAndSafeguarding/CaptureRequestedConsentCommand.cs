using MediatR;

namespace WordUp.Application.ConsentAndSafeguarding;

public sealed record CaptureRequestedConsentCommand(
    string Token,
    string GuardianName,
    string Relationship,
    bool ParticipationConsent,
    bool MediaConsent,
    bool MediaAllPhotosVideo,
    bool MediaGroupOnly,
    bool MediaFirstNameCaptions,
    string Signature) : IRequest<ConsentResult>;

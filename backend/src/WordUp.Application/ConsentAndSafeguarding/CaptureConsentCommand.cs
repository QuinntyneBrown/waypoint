using MediatR;

namespace WordUp.Application.ConsentAndSafeguarding;

public record CaptureConsentCommand(
    Guid YouthUserId,
    string GuardianName,
    string Relationship,
    bool ParticipationConsent,
    bool MediaConsent,
    bool MediaAllPhotosVideo,
    bool MediaGroupOnly,
    bool MediaFirstNameCaptions,
    string Signature,
    string? Term) : IRequest<ConsentResult>, IAtomicOutboxRequest;

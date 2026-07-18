using MediatR;

namespace WordUp.Application.GuardianPortal;

/// <summary>Updates only the media-consent flags on a child's existing consent record. Participation
/// consent is left untouched (and is not required to change media preferences) — its signer/date/validity
/// stay read-only (L2-GUARDIAN-003 AC1).</summary>
public record UpdateMediaConsentCommand(
    Guid ChildId,
    bool MediaConsent,
    bool MediaAllPhotosVideo,
    bool MediaGroupOnly,
    bool MediaFirstNameCaptions) : IRequest;

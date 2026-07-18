using MediatR;

namespace WordUp.Application.GuardianPortal;

/// <summary>The read-only guardian-consent e-sign record(s) for a linked child: signer, date, method,
/// reference id, validity window, participation + granular media flags, and any withdrawal
/// (L2-GUARDIAN-003 AC1/AC3, L2-AUTH-010).</summary>
public record GuardianConsentRecordQuery(Guid ChildId) : IRequest<IReadOnlyList<GuardianConsentRecordView>>;

public record GuardianConsentRecordView(
    Guid ConsentId,
    Guid YouthUserId,
    string GuardianName,
    string Relationship,
    bool ParticipationConsent,
    bool MediaConsent,
    bool MediaAllPhotosVideo,
    bool MediaGroupOnly,
    bool MediaFirstNameCaptions,
    string Method,
    string ReferenceId,
    string? Term,
    DateTime SignedAt,
    DateTime ValidFrom,
    DateTime ValidUntil,
    DateTime? WithdrawnAt);

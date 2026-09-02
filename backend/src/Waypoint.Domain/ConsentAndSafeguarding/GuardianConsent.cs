namespace Waypoint.Domain.ConsentAndSafeguarding;

/// <summary>
/// Guardian consent for a minor youth (L2-CONSENT-001). Participation consent is required and gates
/// program access; media consent is optional with granular, independently-recorded sub-options and is
/// withdrawable. Consent is scoped to a term and re-consent is required each new term.
/// </summary>
public class GuardianConsent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid? GuardianUserId { get; set; }
    public string GuardianName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;

    public bool ParticipationConsent { get; set; }

    // Media / photo consent (optional, granular)
    public bool MediaConsent { get; set; }
    public bool MediaAllPhotosVideo { get; set; }
    public bool MediaGroupOnly { get; set; }
    public bool MediaFirstNameCaptions { get; set; }

    public string Signature { get; set; } = string.Empty;
    public string Method { get; set; } = "typed-e-signature";
    public string ReferenceId { get; set; } = string.Empty;
    public Guid? TermId { get; set; }
    public string? Term { get; set; }
    public DateTime SignedAt { get; set; } = DateTime.UtcNow;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime? WithdrawnAt { get; set; }

    public bool IsParticipationValid(DateTime utcNow) =>
        ParticipationConsent && WithdrawnAt is null && ValidFrom <= utcNow && ValidUntil >= utcNow;
}

namespace WordUp.Domain;

/// <summary>
/// A youth application/referral to join Word Up (L2-INTAKE-001 / L2-INTAKE-002).
/// No account is created at submission; approval provisions the Youth account + enrollment.
/// </summary>
public class YouthApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Neighbourhood { get; set; } = string.Empty;
    public string WhyInterested { get; set; } = string.Empty;
    public string? TrackInterest { get; set; }

    // Guardian details (required when the applicant is a minor)
    public string? GuardianName { get; set; }
    public string? GuardianEmail { get; set; }
    public string? GuardianPhone { get; set; }

    public bool ParticipationConsentAck { get; set; }
    public bool PrivacyAck { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
    public string? DecisionNote { get; set; }
    public Guid? CohortId { get; set; }
    public Guid? MentorId { get; set; }
    public Guid? CreatedYouthUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public bool IsMinor => Age < 18;
}

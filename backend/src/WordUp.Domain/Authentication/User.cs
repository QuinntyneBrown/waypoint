namespace WordUp.Domain.Authentication;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.Youth;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public int SessionVersion { get; set; } = 1;

    // Onboarding (L2-AUTH-011)
    public string? Pronouns { get; set; }
    public string? PhotoUrl { get; set; }
    public bool OnboardingCompleted { get; set; }

    // Program placement (set when provisioned via invitation / application approval)
    public Guid? CohortId { get; set; }
    public Guid? MentorId { get; set; }
    public Guid? GuardianUserId { get; set; }

    // Lockout / brute-force protection (L2-AUTH-006)
    public int FailedSignInAttempts { get; set; }
    public DateTime? LockoutEndsAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSignInAt { get; set; }

    public bool IsLockedOut(DateTime utcNow) => LockoutEndsAt.HasValue && LockoutEndsAt.Value > utcNow;
}

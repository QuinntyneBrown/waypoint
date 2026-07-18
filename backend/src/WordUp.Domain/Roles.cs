namespace WordUp.Domain;

/// <summary>
/// Canonical system roles (L2-AUTH-009). "Lead Mentor" is a per-cohort designation, not a system role.
/// "Public" means an unauthenticated visitor and is therefore not a stored user role.
/// </summary>
public static class Roles
{
    public const string Youth = "Youth";
    public const string Mentor = "Mentor";
    public const string ProgramAdministrator = "ProgramAdministrator";
    public const string Guardian = "Guardian";
    public const string Volunteer = "Volunteer";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string> { Youth, Mentor, ProgramAdministrator, Guardian, Volunteer };

    public static bool IsValid(string role) => All.Contains(role);
}

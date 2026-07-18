namespace WordUp.Domain;

/// <summary>Organization-level settings (L2-ADMIN-006). A single row. The colour palette is fixed
/// platform-wide and is not represented here.</summary>
public class OrgSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Word Up";
    public string? DisplayName { get; set; }
    public string? LogoUrl { get; set; }
    public string? City { get; set; }
    public string? ContactEmail { get; set; }
    public string? ConsentStatement { get; set; }
    public bool RequireReConsentEachTerm { get; set; } = true;
    public bool RequireMediaConsent { get; set; }
}

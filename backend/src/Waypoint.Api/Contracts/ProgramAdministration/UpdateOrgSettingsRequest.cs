namespace Waypoint.Api.Contracts.ProgramAdministration;

public record UpdateOrgSettingsRequest(
    string Name,
    string? DisplayName,
    string? LogoUrl,
    string? City,
    string? ContactEmail,
    string? ConsentStatement,
    bool RequireReConsentEachTerm,
    bool RequireMediaConsent);

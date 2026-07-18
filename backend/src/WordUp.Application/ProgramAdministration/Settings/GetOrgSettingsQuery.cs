using MediatR;

namespace WordUp.Application.ProgramAdministration.Settings;

public record GetOrgSettingsQuery : IRequest<OrgSettingsDto>;

public record OrgSettingsDto(
    string Name,
    string? DisplayName,
    string? LogoUrl,
    string? City,
    string? ContactEmail,
    string? ConsentStatement,
    bool RequireReConsentEachTerm,
    bool RequireMediaConsent);

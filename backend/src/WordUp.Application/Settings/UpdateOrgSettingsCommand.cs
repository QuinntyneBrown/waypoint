using MediatR;

namespace WordUp.Application.Settings;

public record UpdateOrgSettingsCommand(
    string Name,
    string? DisplayName,
    string? LogoUrl,
    string? City,
    string? ContactEmail,
    string? ConsentStatement,
    bool RequireReConsentEachTerm,
    bool RequireMediaConsent) : IRequest;

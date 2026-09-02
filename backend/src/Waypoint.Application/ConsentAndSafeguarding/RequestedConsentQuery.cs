using MediatR;

namespace Waypoint.Application.ConsentAndSafeguarding;

public sealed record RequestedConsentQuery(string Token) : IRequest<RequestedConsentView>;

public sealed record RequestedConsentView(
    string YouthName,
    string GuardianDestination,
    DateTime ExpiresAt,
    string OrganizationName,
    string? ContactEmail,
    string ConsentStatement,
    bool RequireMediaConsent);

using MediatR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.PublicSite;

/// <summary>
/// A public volunteer sign-up (L2-PUB-003 AC2). Submission is only accepted once the visitor has
/// acknowledged that volunteers working with youth must complete a screening and reference check
/// (<see cref="ScreeningConsentAck"/>). Free-text is HTML-encoded server-side (L2-NFR-001).
/// </summary>
public record SubmitVolunteerSignupCommand(
    string Name,
    string Email,
    string? Phone,
    string? AreasOfInterest,
    bool ScreeningConsentAck) : IRequest<SubmitVolunteerSignupResult>, IAtomicRequest;

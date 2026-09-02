using MediatR;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.PublicSite;

/// <summary>
/// A public contact-form submission (L2-PUB-003 AC1). Anonymous; free-text is HTML-encoded server-side
/// before persistence (L2-NFR-001).
/// </summary>
public record SubmitContactRequestCommand(
    string Name,
    string Email,
    string Topic,
    string Message) : IRequest<SubmitContactRequestResult>, IAtomicRequest;

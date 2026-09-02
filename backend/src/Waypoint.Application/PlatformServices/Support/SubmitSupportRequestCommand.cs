using MediatR;

namespace Waypoint.Application.PlatformServices.Support;

/// <summary>Submits a help/support request (L2-PLATFORM-005). The request is persisted and emailed to
/// the program support inbox (the director).</summary>
public record SubmitSupportRequestCommand(
    string Category,
    string Description,
    string ReplyToEmail,
    Guid? ScreenshotFileId) : IRequest<SupportRequestResult>, IAtomicOutboxRequest;

public record SupportRequestResult(Guid Id);

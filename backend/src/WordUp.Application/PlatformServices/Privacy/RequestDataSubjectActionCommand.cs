using MediatR;

namespace WordUp.Application.PlatformServices.Privacy;

/// <summary>
/// Records a data-subject request — access, correction, removal, or consent-withdrawal — for the current
/// authenticated user (L2-NFR-003 AC2). For the "access" right the caller reads their own data via
/// <see cref="MyDataQuery"/> while the durable request remains available for administrator fulfilment.
/// </summary>
public record RequestDataSubjectActionCommand(string ActionType, string? Details)
    : IRequest<DataSubjectRequestResult>;

public record DataSubjectRequestResult(
    Guid Id,
    string Reference,
    Guid SubjectUserId,
    string ActionType,
    string Status,
    DateTime RequestedAt)
{
    public DateTime RecordedAt => RequestedAt;
}

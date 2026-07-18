using MediatR;

namespace WordUp.Application.PlatformServices.Privacy;

/// <summary>
/// Records a data-subject request — access, correction, removal, or consent-withdrawal — for the current
/// authenticated user (L2-NFR-003 AC2). The request is captured as an append-only audit entry; no new
/// entity is introduced. For the "access" right the caller reads their own data via <see cref="MyDataQuery"/>.
/// </summary>
public record RequestDataSubjectActionCommand(string ActionType, string? Details)
    : IRequest<DataSubjectRequestResult>;

public record DataSubjectRequestResult(string Reference, string ActionType, DateTime RecordedAt);

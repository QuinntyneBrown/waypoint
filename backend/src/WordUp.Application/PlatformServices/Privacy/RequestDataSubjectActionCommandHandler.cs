using System.Net;
using MediatR;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.Privacy;

public class RequestDataSubjectActionCommandHandler
    : IRequestHandler<RequestDataSubjectActionCommand, DataSubjectRequestResult>
{
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly IAppDbContext _db;

    public RequestDataSubjectActionCommandHandler(
        ICurrentUser currentUser,
        IClock clock,
        IAuditLogger audit,
        IAppDbContext db)
    {
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
        _db = db;
    }

    public async Task<DataSubjectRequestResult> Handle(
        RequestDataSubjectActionCommand request, CancellationToken cancellationToken)
    {
        // Self-service: the data subject is the authenticated caller. Scope strictly to their own identity.
        var userId = _currentUser.UserId ?? throw new ForbiddenException();

        // Normalise the requested right to its canonical lower-case form so the audit action is stable.
        var actionType = request.ActionType.Trim().ToLowerInvariant();
        var reference = $"DSR-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        var recordedAt = _clock.UtcNow;

        // Free-text detail is HTML-encoded before it is persisted in the encrypted workflow record.
        // The append-only audit entry carries only the opaque reference, never the subject's detail.
        var encodedDetails = string.IsNullOrWhiteSpace(request.Details)
            ? null
            : WebUtility.HtmlEncode(request.Details.Trim());
        var dataRequest = new DataSubjectRequest
        {
            Reference = reference,
            SubjectUserId = userId,
            ActionType = actionType,
            Details = encodedDetails,
            RequestedAt = recordedAt,
        };
        _db.DataSubjectRequests.Add(dataRequest);

        // Record the data-subject request as an append-only audit entry (L2-NFR-003 AC2); actor and target are
        // the current user, since the caller is requesting action on their own data.
        await _audit.LogAsync(
            $"DataSubjectRequest:{actionType}",
            userId,
            "User",
            userId,
            details: $"ref={reference}",
            cancellationToken: cancellationToken);

        return new DataSubjectRequestResult(
            dataRequest.Id,
            reference,
            userId,
            actionType,
            dataRequest.Status.ToString(),
            recordedAt);
    }
}

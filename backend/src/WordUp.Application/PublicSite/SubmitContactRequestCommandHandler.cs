using System.Net;
using MediatR;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Security;

namespace WordUp.Application.PublicSite;

public class SubmitContactRequestCommandHandler : IRequestHandler<SubmitContactRequestCommand, SubmitContactRequestResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly AuthenticatedPrivacyWriteGuard _privacyWriteGuard;

    public SubmitContactRequestCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        AuthenticatedPrivacyWriteGuard privacyWriteGuard)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _privacyWriteGuard = privacyWriteGuard;
    }

    public async Task<SubmitContactRequestResult> Handle(SubmitContactRequestCommand request, CancellationToken cancellationToken)
    {
        var authenticatedAuthorId = await _privacyWriteGuard.AcquireAuthorAsync(cancellationToken);

        // Neutralise stored XSS by HTML-encoding all free-text before persistence (L2-NFR-001, L2-PUB-003 AC4).
        var contactRequest = new ContactRequest
        {
            OwnerUserId = authenticatedAuthorId,
            Name = WebUtility.HtmlEncode(request.Name.Trim()),
            Email = request.Email.Trim().ToLowerInvariant(),
            Topic = WebUtility.HtmlEncode(request.Topic.Trim()),
            Message = WebUtility.HtmlEncode(request.Message.Trim()),
            CreatedAt = _clock.UtcNow,
        };

        _db.ContactRequests.Add(contactRequest);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "ContactRequestSubmitted",
            contactRequest.OwnerUserId,
            "ContactRequest",
            contactRequest.Id);

        return new SubmitContactRequestResult(
            contactRequest.Id,
            "Thanks for reaching out — we've received your message and will be in touch.");
    }
}

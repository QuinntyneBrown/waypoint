using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ConsentAndSafeguarding;

public sealed class CaptureRequestedConsentCommandHandler
    : IRequestHandler<CaptureRequestedConsentCommand, ConsentResult>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;

    public CaptureRequestedConsentCommandHandler(
        IAppDbContext db,
        ISecureTokens tokens,
        IClock clock,
        IEmailSender email,
        IAuditLogger audit)
    {
        _db = db;
        _tokens = tokens;
        _clock = clock;
        _email = email;
        _audit = audit;
    }

    public async Task<ConsentResult> Handle(
        CaptureRequestedConsentCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var tokenHash = _tokens.Hash(request.Token);
        var consentRequest = await _db.GuardianConsentRequests.FirstOrDefaultAsync(
            item => item.TokenHash == tokenHash,
            cancellationToken);
        if (consentRequest is null || !consentRequest.IsUsable(now))
        {
            throw new ConsentRequestLinkInvalidException();
        }

        var youth = await _db.Users.FirstOrDefaultAsync(
            user => user.Id == consentRequest.YouthUserId &&
                    user.GuardianUserId == consentRequest.GuardianUserId,
            cancellationToken);
        var guardian = await _db.Users.FirstOrDefaultAsync(
            user => user.Id == consentRequest.GuardianUserId && user.Role == Roles.Guardian,
            cancellationToken);
        if (youth is null || guardian is null)
        {
            throw new ConsentRequestLinkInvalidException();
        }

        var policy = await ConsentPolicyEvaluator.LoadAsync(_db, now, cancellationToken);
        var referenceId = $"GC-{Guid.NewGuid():N}"[..11].ToUpperInvariant();
        var consent = new GuardianConsent
        {
            YouthUserId = youth.Id,
            GuardianUserId = guardian.Id,
            GuardianName = request.GuardianName.Trim(),
            Relationship = request.Relationship.Trim(),
            ParticipationConsent = request.ParticipationConsent,
            MediaConsent = request.MediaConsent,
            MediaAllPhotosVideo = request.MediaAllPhotosVideo,
            MediaGroupOnly = request.MediaGroupOnly,
            MediaFirstNameCaptions = request.MediaFirstNameCaptions,
            Signature = request.Signature.Trim(),
            Method = "typed-e-signature",
            ReferenceId = referenceId,
            TermId = policy.CurrentTermId,
            Term = policy.CurrentTermName,
            SignedAt = now,
            ValidFrom = now,
            ValidUntil = policy.ConsentValidUntil(now),
        };

        consentRequest.UsedAt = now;
        _db.GuardianConsents.Add(consent);
        await _db.SaveChangesAsync(cancellationToken);

        await _email.SendAsync(
            guardian.Email,
            "Your Word Up consent record",
            $"Thank you. Your consent for {youth.DisplayName} has been recorded. " +
            $"Reference: {consent.ReferenceId}. Participation consent: yes. " +
            $"Media consent: {(consent.MediaConsent ? "yes" : "no")}. " +
            $"Valid until {consent.ValidUntil:yyyy-MM-dd}." +
            CaptureConsentCommandHandler.ContactSentence(policy.ContactEmail),
            cancellationToken,
            subjectUserId: youth.Id,
            recipientUserId: guardian.Id);
        await _audit.LogAsync(
            "GuardianConsentCaptured",
            guardian.Id,
            "GuardianConsent",
            consent.Id,
            cancellationToken: cancellationToken);

        return new ConsentResult(consent.Id, consent.ReferenceId);
    }
}

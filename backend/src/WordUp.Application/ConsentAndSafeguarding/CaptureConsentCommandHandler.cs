using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ConsentAndSafeguarding;

public class CaptureConsentCommandHandler : IRequestHandler<CaptureConsentCommand, ConsentResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CaptureConsentCommandHandler(
        IAppDbContext db,
        IClock clock,
        IEmailSender email,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _email = email;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<ConsentResult> Handle(CaptureConsentCommand request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthUserId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // A guardian may only consent for their OWN child (L2-AUTH-009 / L2-CONSENT).
        if (youth.GuardianUserId is null || youth.GuardianUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("You may only provide consent for your own child.");
        }

        var now = _clock.UtcNow;
        var referenceId = $"GC-{Guid.NewGuid():N}"[..11].ToUpperInvariant();

        var consent = new GuardianConsent
        {
            YouthUserId = youth.Id,
            GuardianUserId = _currentUser.UserId,
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
            Term = request.Term,
            SignedAt = now,
            ValidFrom = now,
            ValidUntil = now.AddYears(1),
        };

        _db.GuardianConsents.Add(consent);
        await _db.SaveChangesAsync(cancellationToken);

        var guardian = await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);
        if (guardian is not null)
        {
            await _email.SendAsync(
                guardian.Email,
                "Your Word Up consent record",
                $"Thank you. Your consent for {youth.DisplayName} has been recorded. " +
                $"Reference: {consent.ReferenceId}. Participation consent: {(consent.ParticipationConsent ? "yes" : "no")}. " +
                $"Media consent: {(consent.MediaConsent ? "yes" : "no")}. Valid until {consent.ValidUntil:yyyy-MM-dd}.",
                cancellationToken);
        }

        await _audit.LogAsync("GuardianConsentCaptured", _currentUser.UserId, "GuardianConsent", consent.Id,
            cancellationToken: cancellationToken);

        return new ConsentResult(consent.Id, consent.ReferenceId);
    }
}

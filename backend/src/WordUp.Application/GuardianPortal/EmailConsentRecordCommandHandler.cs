using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.GuardianPortal;

public sealed class EmailConsentRecordCommandHandler : IRequestHandler<EmailConsentRecordCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;

    public EmailConsentRecordCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IEmailSender email,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _email = email;
        _audit = audit;
    }

    public async Task Handle(EmailConsentRecordCommand request, CancellationToken cancellationToken)
    {
        var guardianId = _currentUser.UserId ?? throw new ForbiddenException();
        var consent = await _db.GuardianConsents.FirstOrDefaultAsync(
                candidate => candidate.Id == request.ConsentId,
                cancellationToken)
            ?? throw new NotFoundException("Consent record not found.");
        if (consent.GuardianUserId != guardianId)
        {
            throw new ForbiddenException("You may only request your own consent record.");
        }

        var guardian = await _db.Users.FirstAsync(user => user.Id == guardianId, cancellationToken);
        var youthName = await _db.Users
            .Where(user => user.Id == consent.YouthUserId)
            .Select(user => user.DisplayName)
            .FirstAsync(cancellationToken);
        await _email.SendAsync(
            guardian.Email,
            "Your Word Up consent record copy",
            $"Consent record for {youthName}. Reference: {consent.ReferenceId}. " +
            $"Signed {consent.SignedAt:yyyy-MM-dd} by {consent.GuardianName} using {consent.Method}. " +
            $"Participation consent: {(consent.ParticipationConsent ? "yes" : "no")}. " +
            $"Media consent: {(consent.MediaConsent ? "yes" : "no")}. " +
            $"Valid {consent.ValidFrom:yyyy-MM-dd} through {consent.ValidUntil:yyyy-MM-dd}. " +
            $"Status: {(consent.WithdrawnAt is null ? "active" : "withdrawn")}.",
            cancellationToken);

        await _audit.LogAsync(
            "ConsentRecordCopyEmailed",
            guardianId,
            "GuardianConsent",
            consent.Id,
            cancellationToken: cancellationToken);
    }
}

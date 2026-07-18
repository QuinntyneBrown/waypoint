using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ConsentAndSafeguarding;

public class WithdrawConsentCommandHandler : IRequestHandler<WithdrawConsentCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public WithdrawConsentCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(WithdrawConsentCommand request, CancellationToken cancellationToken)
    {
        var consent = await _db.GuardianConsents.FirstOrDefaultAsync(c => c.Id == request.ConsentId, cancellationToken)
            ?? throw new NotFoundException("Consent record not found.");

        // A guardian may only withdraw a consent they themselves gave (L2-AUTH-009 / L2-CONSENT).
        if (consent.GuardianUserId is null || consent.GuardianUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("You may only withdraw your own consent.");
        }

        consent.WithdrawnAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("GuardianConsentWithdrawn", _currentUser.UserId, "GuardianConsent", consent.Id,
            cancellationToken: cancellationToken);
    }
}

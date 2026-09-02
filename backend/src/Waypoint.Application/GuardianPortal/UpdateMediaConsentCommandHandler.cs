using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.GuardianPortal;

public class UpdateMediaConsentCommandHandler : IRequestHandler<UpdateMediaConsentCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public UpdateMediaConsentCommandHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task Handle(UpdateMediaConsentCommand request, CancellationToken cancellationToken)
    {
        // Ownership: only the child's own guardian may change media consent (L2-GUARDIAN-003 AC4).
        await GuardianChildLink.ResolveChildAsync(_db, _currentUser, request.ChildId, cancellationToken);

        // Media preferences attach to the child's current (non-withdrawn, most recent) consent record.
        var consent = await _db.GuardianConsents
            .Where(c => c.YouthUserId == request.ChildId && c.WithdrawnAt == null)
            .OrderByDescending(c => c.SignedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("No consent record exists for this child.");

        // Media-only: participation consent and its signer/date/validity are left read-only (AC1).
        consent.MediaConsent = request.MediaConsent;
        consent.MediaAllPhotosVideo = request.MediaAllPhotosVideo;
        consent.MediaGroupOnly = request.MediaGroupOnly;
        consent.MediaFirstNameCaptions = request.MediaFirstNameCaptions;

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("GuardianMediaConsentUpdated", _currentUser.UserId, "GuardianConsent", consent.Id,
            cancellationToken: cancellationToken);
    }
}

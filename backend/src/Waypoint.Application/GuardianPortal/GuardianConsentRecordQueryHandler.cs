using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.GuardianPortal;

public class GuardianConsentRecordQueryHandler
    : IRequestHandler<GuardianConsentRecordQuery, IReadOnlyList<GuardianConsentRecordView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GuardianConsentRecordQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GuardianConsentRecordView>> Handle(
        GuardianConsentRecordQuery request, CancellationToken cancellationToken)
    {
        // Ownership: only the child's own guardian may read the e-sign record (L2-GUARDIAN-003 AC4).
        await GuardianChildLink.ResolveChildAsync(_db, _currentUser, request.ChildId, cancellationToken);

        return await _db.GuardianConsents
            .Where(c => c.YouthUserId == request.ChildId)
            .OrderByDescending(c => c.SignedAt)
            .Select(c => new GuardianConsentRecordView(
                c.Id,
                c.YouthUserId,
                c.GuardianName,
                c.Relationship,
                c.ParticipationConsent,
                c.MediaConsent,
                c.MediaAllPhotosVideo,
                c.MediaGroupOnly,
                c.MediaFirstNameCaptions,
                c.Method,
                c.ReferenceId,
                c.Term,
                c.SignedAt,
                c.ValidFrom,
                c.ValidUntil,
                c.WithdrawnAt))
            .ToListAsync(cancellationToken);
    }
}

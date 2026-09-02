using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Badges;

public class DeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public DeleteBadgeCommandHandler(
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

    public async Task Handle(DeleteBadgeCommand request, CancellationToken cancellationToken)
    {
        var badge = await _db.Badges.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Badge not found.");

        // Archive rather than hard-delete so existing awards are preserved (L2-CURRIC-005 AC4).
        badge.ArchivedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("BadgeArchived", _currentUser.UserId, "Badge", badge.Id);
    }
}

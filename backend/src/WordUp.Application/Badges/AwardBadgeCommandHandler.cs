using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Badges;

public class AwardBadgeCommandHandler : IRequestHandler<AwardBadgeCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public AwardBadgeCommandHandler(
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

    public async Task Handle(AwardBadgeCommand request, CancellationToken cancellationToken)
    {
        var badge = await _db.Badges.FirstOrDefaultAsync(b => b.Id == request.BadgeId, cancellationToken)
            ?? throw new NotFoundException("Badge not found.");

        // A badge is awarded to a youth at most once (L2-CURRIC-005 AC3).
        var alreadyAwarded = await _db.BadgeAwards.AnyAsync(
            a => a.BadgeId == request.BadgeId && a.YouthUserId == request.YouthUserId, cancellationToken);
        if (alreadyAwarded)
        {
            return;
        }

        _db.BadgeAwards.Add(new BadgeAward
        {
            BadgeId = badge.Id,
            YouthUserId = request.YouthUserId,
            AwardedByUserId = _currentUser.UserId,
            AwardedAt = _clock.UtcNow,
        });

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("BadgeAwarded", _currentUser.UserId, "Badge", badge.Id);
    }
}

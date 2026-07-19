using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Badges;

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

        if (badge.ArchivedAt is not null)
        {
            throw new ConflictException("Archived badges cannot be awarded.");
        }

        if (badge.CriteriaKind != BadgeCriteriaKind.Manual)
        {
            throw new ConflictException("Automatic badges cannot be awarded manually.");
        }

        var youth = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == request.YouthUserId
                    && item.Role == Roles.Youth
                    && item.Status == UserStatus.Active,
                cancellationToken)
            ?? throw new NotFoundException("Active youth not found.");
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("Mentors may award badges only to their assigned youth.");
        }

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
        await _audit.LogAsync(
            "BadgeAwarded",
            _currentUser.UserId,
            "Badge",
            badge.Id,
            cancellationToken: cancellationToken);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Announcements;

public class ComposeAnnouncementCommandHandler : IRequestHandler<ComposeAnnouncementCommand, AnnouncementResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public ComposeAnnouncementCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
    }

    public async Task<AnnouncementResult> Handle(ComposeAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        if (request.CohortId is Guid cohortId
            && !await _db.Cohorts.AsNoTracking().AnyAsync(item => item.Id == cohortId, cancellationToken))
        {
            throw new NotFoundException("Cohort not found.");
        }

        // Send now when there is no future schedule; otherwise leave SentAt null until the scheduled time.
        DateTime? sentAt = request.ScheduledAt is null || request.ScheduledAt <= now ? now : null;

        var announcement = new Announcement
        {
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            AudienceScope = request.AudienceScope,
            CohortId = request.CohortId,
            RoleTargeting = request.RoleTargeting,
            ScheduledAt = request.ScheduledAt,
            SentAt = sentAt,
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = now,
        };

        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("AnnouncementComposed", _currentUser.UserId, "Announcement", announcement.Id, cancellationToken: cancellationToken);

        return new AnnouncementResult(announcement.Id, announcement.SentAt);
    }
}

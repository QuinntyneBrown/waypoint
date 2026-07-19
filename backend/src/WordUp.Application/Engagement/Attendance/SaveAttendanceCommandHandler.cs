using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Attendance;

public class SaveAttendanceCommandHandler : IRequestHandler<SaveAttendanceCommand, AttendanceTally>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;
    private readonly IChangePublisher _publisher;

    public SaveAttendanceCommandHandler(
        IAppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IAuditLogger audit,
        IChangePublisher publisher)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
        _publisher = publisher;
    }

    public async Task<AttendanceTally> Handle(SaveAttendanceCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session not found.");

        var recordedBy = _currentUser.UserId!.Value;
        var now = _clock.UtcNow;

        if (_currentUser.Role == Roles.Volunteer && session.VolunteerId != recordedBy)
        {
            throw new ForbiddenException("This session is not assigned to you.");
        }
        if (_currentUser.Role == Roles.Mentor && session.MentorId != recordedBy)
        {
            throw new ForbiddenException("This session is not assigned to you.");
        }

        var fingerprint = Fingerprint(request);
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var receipt = await _db.AttendanceWriteReceipts.FirstOrDefaultAsync(
                r => r.OwnerUserId == recordedBy && r.IdempotencyKey == request.IdempotencyKey,
                cancellationToken);
            if (receipt is not null)
            {
                if (receipt.RequestFingerprint != fingerprint)
                {
                    throw new ConflictException(
                        "That attendance operation key was already used for different changes.");
                }

                return new AttendanceTally(
                    receipt.Present,
                    receipt.Late,
                    receipt.Excused,
                    receipt.Absent,
                    receipt.Marked,
                    receipt.Total);
            }
        }

        // Parse statuses and enforce the volunteer restriction up-front so nothing is written on rejection
        // (volunteers may only mark Present/Absent — L2-ENGAGE-002).
        var parsed = new List<(Guid YouthId, AttendanceStatus Status, string? Note, string? ExpectedVersion)>();
        foreach (var mark in request.Marks)
        {
            var status = Enum.Parse<AttendanceStatus>(mark.Status, ignoreCase: true);
            if (_currentUser.Role == Roles.Volunteer && status is AttendanceStatus.Late or AttendanceStatus.Excused)
            {
                throw new ForbiddenException("Volunteers may only mark youth Present or Absent.");
            }
            if (_currentUser.Role == Roles.Volunteer && !string.IsNullOrWhiteSpace(mark.Note))
            {
                throw new ForbiddenException("Volunteers may not add youth notes.");
            }
            parsed.Add((mark.YouthId, status, mark.Note, mark.ExpectedVersion));
        }

        var youthIds = parsed.Select(p => p.YouthId).Distinct().ToList();
        var allowedYouthCount = await _db.Users.CountAsync(
            u => youthIds.Contains(u.Id)
                && u.CohortId == session.CohortId
                && u.Role == Roles.Youth
                && u.Status == UserStatus.Active,
            cancellationToken);
        if (allowedYouthCount != youthIds.Count)
        {
            throw new ForbiddenException("One or more youth are outside this session's roster.");
        }

        var sessionRecords = await _db.AttendanceRecords
            .Where(r => r.SessionId == request.SessionId)
            .ToListAsync(cancellationToken);
        var existing = sessionRecords.Where(r => youthIds.Contains(r.YouthUserId)).ToList();

        // Optimistic concurrency: if a client supplies the version it last read, reject the whole save
        // when any target row has moved on since — no silent overwrite (L2-NFR-006 AC1). Checked before
        // any mutation so nothing is written on conflict.
        foreach (var (youthId, _, _, expectedVersion) in parsed)
        {
            if (expectedVersion is null)
            {
                continue;
            }

            var current = existing.FirstOrDefault(r => r.YouthUserId == youthId);
            var currentVersion = current?.ConcurrencyStamp.ToString();
            var matches = expectedVersion == "none"
                ? current is null
                : currentVersion == expectedVersion;
            if (!matches)
            {
                throw new ConflictException("This attendance record was changed by someone else. Reload and try again.");
            }
        }

        // Upsert by (SessionId, YouthUserId): re-saving updates the existing row rather than inserting a
        // duplicate, which is what makes offline sync / re-save idempotent (L2-ENGAGE-003 AC2).
        foreach (var (youthId, status, note, _) in parsed)
        {
            var record = existing.FirstOrDefault(r => r.YouthUserId == youthId);
            if (record is null)
            {
                record = new AttendanceRecord
                {
                    SessionId = request.SessionId,
                    YouthUserId = youthId,
                    Status = status,
                    Note = note,
                    RecordedByUserId = recordedBy,
                    RecordedAt = now,
                    ConcurrencyStamp = Guid.NewGuid(),
                };
                _db.AttendanceRecords.Add(record);
                existing.Add(record); // guard against the same youth appearing twice in one request
                sessionRecords.Add(record);
            }
            else
            {
                record.Status = status;
                record.Note = note;
                record.RecordedByUserId = recordedBy;
                record.RecordedAt = now;
                record.ConcurrencyStamp = Guid.NewGuid(); // advance the version on every save (L2-NFR-006 AC2)
            }
        }

        var present = sessionRecords.Count(r => r.Status == AttendanceStatus.Present);
        var late = sessionRecords.Count(r => r.Status == AttendanceStatus.Late);
        var excused = sessionRecords.Count(r => r.Status == AttendanceStatus.Excused);
        var absent = sessionRecords.Count(r => r.Status == AttendanceStatus.Absent);
        var marked = sessionRecords.Count;

        var total = await _db.Users
            .CountAsync(u => u.CohortId == session.CohortId
                && u.Role == Roles.Youth
                && u.Status == UserStatus.Active, cancellationToken);

        var tally = new AttendanceTally(present, late, excused, absent, marked, total);
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            _db.AttendanceWriteReceipts.Add(new AttendanceWriteReceipt
            {
                OwnerUserId = recordedBy,
                IdempotencyKey = request.IdempotencyKey,
                RequestFingerprint = fingerprint,
                Present = tally.Present,
                Late = tally.Late,
                Excused = tally.Excused,
                Absent = tally.Absent,
                Marked = tally.Marked,
                Total = tally.Total,
                CreatedAt = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await AwardAttendanceStreakBadgesAsync(youthIds, now, cancellationToken);
        await _audit.LogAsync(
            "AttendanceSaved",
            recordedBy,
            "Session",
            request.SessionId,
            cancellationToken: cancellationToken);

        return tally;
    }

    private async Task AwardAttendanceStreakBadgesAsync(
        IReadOnlyCollection<Guid> youthIds,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var badges = await _db.Badges
            .Where(item => item.CriteriaKind == BadgeCriteriaKind.Auto
                && item.AutoRule == "attendance-streak"
                && item.ArchivedAt == null)
            .ToListAsync(cancellationToken);
        if (badges.Count == 0)
        {
            return;
        }

        var badgeIds = badges.Select(item => item.Id).ToList();
        var notifications = new List<Notification>();
        foreach (var youthId in youthIds)
        {
            var statuses = await (
                from record in _db.AttendanceRecords
                join session in _db.Sessions on record.SessionId equals session.Id
                where record.YouthUserId == youthId
                orderby session.StartsAt descending
                select record.Status)
                .ToListAsync(cancellationToken);
            var streak = 0;
            foreach (var status in statuses)
            {
                if (status is AttendanceStatus.Present or AttendanceStatus.Late)
                {
                    streak++;
                }
                else if (status == AttendanceStatus.Absent)
                {
                    break;
                }
            }

            if (streak < 5)
            {
                continue;
            }

            var earnedBadgeIds = await _db.BadgeAwards
                .Where(item => item.YouthUserId == youthId && badgeIds.Contains(item.BadgeId))
                .Select(item => item.BadgeId)
                .ToListAsync(cancellationToken);
            foreach (var badge in badges.Where(item => !earnedBadgeIds.Contains(item.Id)))
            {
                _db.BadgeAwards.Add(new BadgeAward
                {
                    BadgeId = badge.Id,
                    YouthUserId = youthId,
                    AwardedAt = now,
                });
                var notification = new Notification
                {
                    UserId = youthId,
                    EventKey = "badge-earned",
                    Title = $"Badge earned: {badge.Name}",
                    Body = string.IsNullOrWhiteSpace(badge.Description)
                        ? "You earned a new badge."
                        : badge.Description,
                    LinkPath = "/youth/journey",
                    CreatedAt = now,
                };
                _db.Notifications.Add(notification);
                notifications.Add(notification);
            }
        }

        if (notifications.Count == 0)
        {
            return;
        }

        await _db.SaveChangesAsync(cancellationToken);
        foreach (var notification in notifications)
        {
            await _publisher.PublishToUserAsync(
                notification.UserId,
                "notification",
                new
                {
                    id = notification.Id,
                    eventKey = notification.EventKey,
                    title = notification.Title,
                    body = notification.Body,
                    linkPath = notification.LinkPath,
                    createdAt = notification.CreatedAt,
                },
                cancellationToken);
        }
    }

    private static string Fingerprint(SaveAttendanceCommand request)
    {
        var normalized = string.Join(
            "|",
            request.Marks.Select(mark =>
                $"{mark.YouthId:D}:{mark.Status.ToUpperInvariant()}:{mark.Note}:{mark.ExpectedVersion}"));
        var bytes = Encoding.UTF8.GetBytes($"{request.SessionId:D}|{normalized}");
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.LearningExperience.YouthCurriculum;

namespace Waypoint.Application.GuardianPortal;

public class ChildProgressQueryHandler : IRequestHandler<ChildProgressQuery, ChildProgressView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public ChildProgressQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<ChildProgressView> Handle(ChildProgressQuery request, CancellationToken cancellationToken)
    {
        // Ownership: a guardian may only view their own linked child (L2-GUARDIAN-001 AC3 / L2-AUTH-010).
        var child = await GuardianChildLink.ResolveChildAsync(_db, _currentUser, request.ChildId, cancellationToken);

        // Per-module status reuses the single source of truth used by the youth curriculum view. This
        // exposes only aggregate completion + gating — no lesson body, quiz answers, or reflections (AC2).
        IReadOnlyList<ModuleStatus> modules = Array.Empty<ModuleStatus>();
        var trackId = await CurriculumProgressCalculator.ResolveYouthTrackIdAsync(_db, child.Id, cancellationToken);
        if (trackId is { } tid)
        {
            modules = await CurriculumProgressCalculator.BuildModuleStatusesAsync(_db, tid, child.Id, cancellationToken);
        }

        var overall = modules.Count == 0 ? 0d : modules.Average(m => m.CompletionPercent);

        // Attendance tally (present/late/excused/absent) — the same status set as ENGAGE reporting.
        var statuses = await _db.AttendanceRecords
            .Where(r => r.YouthUserId == child.Id)
            .Select(r => r.Status)
            .ToListAsync(cancellationToken);
        var present = statuses.Count(s => s == AttendanceStatus.Present);
        var late = statuses.Count(s => s == AttendanceStatus.Late);
        var excused = statuses.Count(s => s == AttendanceStatus.Excused);
        var absent = statuses.Count(s => s == AttendanceStatus.Absent);
        var total = statuses.Count;
        var attended = present + late;
        var rate = total > 0 ? (double)attended / total : 0d;
        var attendance = new ChildAttendanceSummary(present, late, excused, absent, total, rate);
        var attendanceRows = await (
                from record in _db.AttendanceRecords
                join session in _db.Sessions on record.SessionId equals session.Id
                where record.YouthUserId == child.Id
                orderby session.StartsAt descending
                select new
                {
                    SessionId = session.Id,
                    session.Title,
                    session.StartsAt,
                    session.Location,
                    record.Status,
                })
            .ToListAsync(cancellationToken);
        var attendanceHistory = attendanceRows
            .Select(row => new ChildAttendanceItem(
                row.SessionId,
                row.Title,
                row.StartsAt,
                row.Location,
                row.Status.ToString()))
            .ToList();
        var quizHistory = await ProgressEvidenceCalculator.QuizHistoryAsync(
            _db, child.Id, cancellationToken);
        var competencies = await ProgressEvidenceCalculator.CompetenciesAsync(
            _db, child.Id, cancellationToken);

        // Earned vs locked badges: the active badge catalogue split by what the child already holds.
        var earnedSet = (await _db.BadgeAwards
                .Where(a => a.YouthUserId == child.Id)
                .Select(a => a.BadgeId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var badges = await _db.Badges
            .Where(b => b.ArchivedAt == null)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
        var earned = badges.Where(b => earnedSet.Contains(b.Id))
            .Select(b => new ChildBadge(b.Id, b.Name, b.Icon, b.Description))
            .ToList();
        var locked = badges.Where(b => !earnedSet.Contains(b.Id))
            .Select(b => new ChildBadge(b.Id, b.Name, b.Icon, b.Description))
            .ToList();

        // Upcoming schedule = future sessions for the child's cohort.
        var now = _clock.UtcNow;
        List<ChildUpcomingSession> upcoming;
        if (child.CohortId is { } cohortId)
        {
            upcoming = await _db.Sessions
                .Where(s => s.CohortId == cohortId && s.StartsAt >= now)
                .OrderBy(s => s.StartsAt)
                .Select(s => new ChildUpcomingSession(s.Id, s.Title, s.StartsAt, s.Location))
                .ToListAsync(cancellationToken);
        }
        else
        {
            upcoming = new List<ChildUpcomingSession>();
        }

        // Only explicitly shared check-ins are returned. The selected fields are the guardian-safe
        // shared content; private entries are excluded at the query boundary (GUARDIAN-001 AC4).
        var sharedCheckIns = await _db.CheckIns
            .Where(checkIn => checkIn.YouthUserId == child.Id
                && checkIn.Visibility == Visibility.SharedWithMentor)
            .OrderByDescending(checkIn => checkIn.Date)
            .Select(checkIn => new GuardianSharedCheckIn(
                checkIn.Id,
                checkIn.Date,
                checkIn.Type,
                checkIn.Engagement,
                checkIn.GoalsDiscussed,
                checkIn.PrayerRequests,
                checkIn.ActionItems,
                checkIn.NextFollowUp))
            .ToListAsync(cancellationToken);

        return new ChildProgressView(
            child.Id,
            child.DisplayName,
            overall,
            modules,
            attendance,
            attendanceHistory,
            quizHistory,
            competencies,
            earned,
            locked,
            upcoming,
            sharedCheckIns);
    }
}

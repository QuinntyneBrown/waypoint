using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.ConsentAndSafeguarding;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;
using WordUp.Application.Mentoring.Flags;

namespace WordUp.Application.Mentoring.MentorRoster;

public class YouthDetailQueryHandler : IRequestHandler<YouthDetailQuery, YouthDetail>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly AtRiskEvaluator _atRiskEvaluator;
    private readonly IClock _clock;

    public YouthDetailQueryHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        AtRiskEvaluator atRiskEvaluator,
        IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _atRiskEvaluator = atRiskEvaluator;
        _clock = clock;
    }

    public async Task<YouthDetail> Handle(YouthDetailQuery request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // A mentor may only view their assigned youth; a Program Administrator may view any (L2-MENTOR-002 AC3, L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        // Latest active flag for the at-risk banner (L2-MENTOR-002 AC2).
        var flag = await _db.Flags
            .Where(f => f.YouthUserId == youth.Id && f.ResolvedAt == null)
            .OrderByDescending(f => f.FlaggedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var cohort = youth.CohortId is { } cohortId
            ? await _db.Cohorts.FirstOrDefaultAsync(item => item.Id == cohortId, cancellationToken)
            : null;
        var guardianUser = youth.GuardianUserId is { } guardianId
            ? await _db.Users.FirstOrDefaultAsync(item => item.Id == guardianId, cancellationToken)
            : null;
        var guardianProfile = guardianUser is not null
            ? await _db.GuardianProfiles.FirstOrDefaultAsync(
                item => item.GuardianUserId == guardianUser.Id,
                cancellationToken)
            : null;
        var guardian = guardianUser is null
            ? null
            : new YouthGuardianContact(
                guardianUser.DisplayName,
                guardianUser.Email,
                guardianProfile?.Phone,
                guardianProfile?.EmergencyContactName,
                guardianProfile?.EmergencyContactPhone);

        var trackIds = await _db.YouthTrackEnrollments
            .Where(item => item.YouthUserId == youth.Id)
            .Select(item => item.TrackId)
            .ToListAsync(cancellationToken);
        if (cohort?.TrackId is { } cohortTrackId && !trackIds.Contains(cohortTrackId))
        {
            trackIds.Add(cohortTrackId);
        }

        await _atRiskEvaluator.EvaluateAsync(new[] { youth.Id }, cancellationToken: cancellationToken);
        var moduleIds = await _db.Modules
            .Where(item => trackIds.Contains(item.TrackId) && item.Status == PublishStatus.Published)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var lessonIds = await _db.Lessons
            .Where(item => moduleIds.Contains(item.ModuleId)
                && item.Status == PublishStatus.Published
                && item.ArchivedAt == null)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var completedLessons = await _db.LessonProgress
            .CountAsync(item => item.YouthUserId == youth.Id
                && lessonIds.Contains(item.LessonId)
                && item.Status == ProgressStatus.Completed,
                cancellationToken);
        var progress = new YouthProgressSummary(
            completedLessons,
            lessonIds.Count,
            lessonIds.Count == 0 ? 0 : (double)completedLessons / lessonIds.Count);

        var attendanceRows = await _db.AttendanceRecords
            .Where(item => item.YouthUserId == youth.Id)
            .OrderByDescending(item => item.RecordedAt)
            .ToListAsync(cancellationToken);
        var sessionIds = attendanceRows.Select(item => item.SessionId).Distinct().ToList();
        var sessions = await _db.Sessions
            .Where(item => sessionIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var attendance = attendanceRows
            .Where(item => sessions.ContainsKey(item.SessionId))
            .Select(item => new YouthAttendanceSummary(
                item.SessionId,
                sessions[item.SessionId].Title,
                sessions[item.SessionId].StartsAt,
                item.Status.ToString()))
            .ToList();

        var checkInQuery = _db.CheckIns.Where(item => item.YouthUserId == youth.Id);
        if (_currentUser.Role == Roles.Mentor)
        {
            checkInQuery = checkInQuery.Where(item => item.MentorUserId == _currentUser.UserId);
        }
        var checkIns = await checkInQuery
            .OrderByDescending(item => item.Date)
            .Select(item => new YouthCheckInSummary(
                item.Id,
                item.Date,
                item.Type,
                item.Visibility.ToString(),
                item.NextFollowUp))
            .ToListAsync(cancellationToken);

        var submissionRows = await _db.Submissions
            .Where(item => item.YouthUserId == youth.Id)
            .OrderByDescending(item => item.SubmittedAt)
            .ToListAsync(cancellationToken);
        var assignmentIds = submissionRows.Select(item => item.AssignmentId).Distinct().ToList();
        var assignments = await _db.Assignments
            .Where(item => assignmentIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var submissions = submissionRows
            .Select(item => new YouthSubmissionSummary(
                item.Id,
                assignments.TryGetValue(item.AssignmentId, out var assignment)
                    ? assignment.Title
                    : "Assignment",
                item.Status.ToString(),
                item.SubmittedAt))
            .ToList();

        var assessmentRows = await _db.CompetencyAssessments
            .Where(item => item.YouthUserId == youth.Id)
            .ToListAsync(cancellationToken);
        var latestAssessments = assessmentRows
            .GroupBy(item => item.CompetencyId)
            .Select(group => group.OrderByDescending(item => item.AssessedAt).First())
            .OrderBy(item => item.AssessedAt)
            .ToList();
        var competencyIds = latestAssessments.Select(item => item.CompetencyId).ToList();
        var competencyNames = await _db.Competencies
            .Where(item => competencyIds.Contains(item.Id) && item.ArchivedAt == null)
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var competencies = latestAssessments
            .Where(item => competencyNames.ContainsKey(item.CompetencyId))
            .Select(item => new YouthCompetencySummary(
                item.CompetencyId,
                competencyNames.TryGetValue(item.CompetencyId, out var name) ? name : "Competency",
                item.Level.ToString(),
                item.Notes,
                item.AssessedAt))
            .ToList();

        var flaggedByName = flag is null
            ? null
            : flag.IsAutomatic
                ? "Automated evaluation"
                : await _db.Users
                    .Where(item => item.Id == flag.FlaggedByUserId)
                    .Select(item => item.DisplayName)
                    .FirstOrDefaultAsync(cancellationToken);
        var photoUrl = youth.PhotoUrl;
        if (photoUrl is not null
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                youth.Id,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            photoUrl = null;
        }

        return new YouthDetail(
            youth.Id,
            youth.DisplayName,
            youth.Email,
            youth.Pronouns,
            photoUrl,
            youth.CohortId,
            cohort?.Name,
            youth.GuardianUserId,
            guardian,
            flag is not null,
            flag?.Reason,
            flag?.FlaggedByUserId,
            flaggedByName,
            flag?.FlaggedAt,
            progress,
            attendance,
            checkIns,
            submissions,
            competencies);
    }
}

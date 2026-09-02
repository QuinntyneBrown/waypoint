using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.LearningExperience.CodingLessons;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.LearningExperience.YouthCurriculum;

public class JourneyQueryHandler : IRequestHandler<JourneyQuery, JourneyView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public JourneyQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<JourneyView> Handle(JourneyQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var youth = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == youthId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        IReadOnlyList<ModuleStatus> modules = [];
        var trackId = await CurriculumProgressCalculator.ResolveYouthTrackIdAsync(_db, youthId, cancellationToken);
        if (trackId is { } selectedTrackId)
        {
            modules = await CurriculumProgressCalculator.BuildModuleStatusesAsync(
                _db, selectedTrackId, youthId, cancellationToken);
        }

        var allDefinitions = await _db.Badges.AsNoTracking()
            .Where(item => item.ArchivedAt == null)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        var definitions = await RelevantDefinitionsAsync(
            youthId, allDefinitions, cancellationToken);
        var awards = await _db.BadgeAwards.AsNoTracking()
            .Where(item => item.YouthUserId == youthId)
            .OrderBy(item => item.AwardedAt)
            .ToListAsync(cancellationToken);
        var definitionById = definitions.ToDictionary(item => item.Id);
        var earned = awards
            .Where(item => definitionById.ContainsKey(item.BadgeId))
            .Select(item =>
            {
                var badge = definitionById[item.BadgeId];
                return new EarnedBadge(
                    badge.Id,
                    badge.Name,
                    badge.Icon,
                    CriteriaFor(badge),
                    item.AwardedAt);
            })
            .OrderByDescending(item => item.AwardedAt)
            .ToList();
        var earnedIds = earned.Select(item => item.BadgeId).ToHashSet();

        var progressData = await LoadProgressDataAsync(youthId, definitions, cancellationToken);
        var locked = definitions
            .Where(item => !earnedIds.Contains(item.Id))
            .Select(item =>
            {
                var count = CountFor(item, modules, progressData);
                return new LockedBadge(
                    item.Id,
                    item.Name,
                    item.Icon,
                    CriteriaFor(item),
                    count.Completed,
                    count.Total,
                    $"{count.Completed} of {count.Total}");
            })
            .ToList();

        var milestones = await BuildMilestonesAsync(youth, earned, locked, cancellationToken);
        return new JourneyView(modules, earned, locked, milestones);
    }

    private async Task<List<Badge>> RelevantDefinitionsAsync(
        Guid youthId,
        IReadOnlyList<Badge> definitions,
        CancellationToken cancellationToken)
    {
        var trackIds = await CurriculumProgressCalculator.ResolveYouthTrackIdsAsync(
            _db, youthId, cancellationToken);
        var moduleIds = await _db.Modules.AsNoTracking()
            .Where(item => trackIds.Contains(item.TrackId))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var lessonIds = await _db.Lessons.AsNoTracking()
            .Where(item => moduleIds.Contains(item.ModuleId))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var quizIds = await _db.Quizzes.AsNoTracking()
            .Where(item => item.ModuleId != null && moduleIds.Contains(item.ModuleId.Value))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var assignmentIds = await _db.Assignments.AsNoTracking()
            .Where(item => item.ModuleId != null && moduleIds.Contains(item.ModuleId.Value))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        return definitions.Where(badge => badge.TargetRefId is not { } targetId || badge.AutoRule switch
        {
            "module-completed" => moduleIds.Contains(targetId),
            "lesson-completed" or "coding-checklist-completed" => lessonIds.Contains(targetId),
            "quiz-passed" => quizIds.Contains(targetId),
            "assignment-approved" => assignmentIds.Contains(targetId),
            _ => true,
        }).ToList();
    }

    private async Task<BadgeProgressData> LoadProgressDataAsync(
        Guid youthId,
        IReadOnlyList<Badge> definitions,
        CancellationToken cancellationToken)
    {
        var completedLessonIds = await _db.LessonProgress.AsNoTracking()
            .Where(item => item.YouthUserId == youthId && item.Status == ProgressStatus.Completed)
            .Select(item => item.LessonId)
            .ToListAsync(cancellationToken);
        var attempts = await _db.QuizAttempts.AsNoTracking()
            .Where(item => item.YouthUserId == youthId && item.ApprovedScore != null)
            .ToListAsync(cancellationToken);
        var attemptedQuizIds = attempts.Select(item => item.QuizId).Distinct().ToList();
        var passMarks = await _db.Quizzes.AsNoTracking()
            .Where(item => attemptedQuizIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.PassMark, cancellationToken);
        var passedQuizIds = attempts
            .Where(item => passMarks.TryGetValue(item.QuizId, out var passMark)
                && item.ApprovedScore >= passMark)
            .Select(item => item.QuizId)
            .ToHashSet();
        var approvedAssignmentIds = await _db.Submissions.AsNoTracking()
            .Where(item => item.YouthUserId == youthId && item.Status == SubmissionStatus.Approved)
            .Select(item => item.AssignmentId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var workspaces = await _db.CodingLessonWorkspaces.AsNoTracking()
            .Where(item => item.YouthUserId == youthId)
            .ToListAsync(cancellationToken);
        var codingLessonIds = definitions
            .Where(item => item.AutoRule == "coding-checklist-completed" && item.TargetRefId != null)
            .Select(item => item.TargetRefId!.Value)
            .Concat(workspaces.Select(item => item.LessonId))
            .Distinct()
            .ToList();
        var stepCounts = await _db.CodingLessonSteps.AsNoTracking()
            .Where(item => codingLessonIds.Contains(item.LessonId))
            .GroupBy(item => item.LessonId)
            .Select(group => new { LessonId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.LessonId, item => item.Count, cancellationToken);
        var checkedCounts = workspaces.ToDictionary(
            item => item.LessonId,
            item => CodingLessonQueryHandler.ParseIds(item.CheckedStepIdsJson).Distinct().Count());
        var attendance = await (
            from record in _db.AttendanceRecords.AsNoTracking()
            join session in _db.Sessions.AsNoTracking() on record.SessionId equals session.Id
            where record.YouthUserId == youthId
            orderby session.StartsAt descending
            select record.Status)
            .ToListAsync(cancellationToken);
        var attendanceStreak = 0;
        foreach (var status in attendance)
        {
            if (status is AttendanceStatus.Present or AttendanceStatus.Late) attendanceStreak++;
            else if (status == AttendanceStatus.Absent) break;
        }

        return new BadgeProgressData(
            completedLessonIds.ToHashSet(),
            passedQuizIds,
            approvedAssignmentIds.ToHashSet(),
            stepCounts,
            checkedCounts,
            attendanceStreak);
    }

    private static BadgeCount CountFor(
        Badge badge,
        IReadOnlyList<ModuleStatus> modules,
        BadgeProgressData data)
    {
        return badge.AutoRule switch
        {
            "lesson-completed" => new(
                badge.TargetRefId is { } lessonId
                    ? Convert.ToInt32(data.CompletedLessonIds.Contains(lessonId))
                    : Math.Min(1, data.CompletedLessonIds.Count),
                1),
            "module-completed" when badge.TargetRefId is { } moduleId =>
                modules.FirstOrDefault(item => item.ModuleId == moduleId) is { } module
                    ? new(module.CompletedItems, Math.Max(1, module.TotalItems))
                    : new(0, 1),
            "quiz-passed" => new(
                badge.TargetRefId is { } quizId && data.PassedQuizIds.Contains(quizId) ? 1 : 0,
                1),
            "assignment-approved" => new(
                badge.TargetRefId is { } assignmentId && data.ApprovedAssignmentIds.Contains(assignmentId) ? 1 : 0,
                1),
            "coding-checklist-completed" when badge.TargetRefId is { } codingLessonId => new(
                data.CheckedCodingSteps.GetValueOrDefault(codingLessonId),
                Math.Max(1, data.CodingStepTotals.GetValueOrDefault(codingLessonId, 1))),
            "attendance-streak" => new(Math.Min(data.AttendanceStreak, 5), 5),
            _ => new(0, 1),
        };
    }

    private async Task<IReadOnlyList<JourneyMilestone>> BuildMilestonesAsync(
        User youth,
        IReadOnlyList<EarnedBadge> earned,
        IReadOnlyList<LockedBadge> locked,
        CancellationToken cancellationToken)
    {
        var milestones = new List<JourneyMilestone>();
        if (youth.CohortId is { } cohortId)
        {
            var cohort = await _db.Cohorts.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == cohortId, cancellationToken);
            if (cohort is not null)
            {
                var joinedAt = cohort.TermStart == default ? youth.CreatedAt : cohort.TermStart;
                milestones.Add(new(
                    "Joined",
                    $"Joined {cohort.Name}",
                    "Welcome to Waypoint",
                    joinedAt,
                    false));
            }
        }
        foreach (var badge in earned.OrderBy(item => item.AwardedAt))
        {
            milestones.Add(new(
                "BadgeEarned",
                $"{badge.Name} badge",
                badge.Criteria,
                badge.AwardedAt,
                false));
        }
        var next = locked
            .OrderByDescending(item => (double)item.CompletedCriteria / Math.Max(1, item.TotalCriteria))
            .ThenBy(item => item.Name)
            .FirstOrDefault();
        if (next is not null)
        {
            milestones.Add(new(
                "NextBadge",
                $"Next up: {next.Name}",
                $"{next.ProgressLabel} · {next.Criteria}",
                null,
                true));
        }
        return milestones;
    }

    private static string CriteriaFor(Badge badge) => badge.Description ?? badge.AutoRule switch
    {
        "lesson-completed" => "Complete the lesson",
        "module-completed" => "Complete every required item in the module",
        "quiz-passed" => "Pass the quiz",
        "assignment-approved" => "Have the assignment approved",
        "coding-checklist-completed" => "Complete every coding self-check",
        "attendance-streak" => "Attend five sessions in a row",
        _ => "Awarded by a mentor",
    };

    private sealed record BadgeProgressData(
        IReadOnlySet<Guid> CompletedLessonIds,
        IReadOnlySet<Guid> PassedQuizIds,
        IReadOnlySet<Guid> ApprovedAssignmentIds,
        IReadOnlyDictionary<Guid, int> CodingStepTotals,
        IReadOnlyDictionary<Guid, int> CheckedCodingSteps,
        int AttendanceStreak);

    private sealed record BadgeCount(int Completed, int Total);
}

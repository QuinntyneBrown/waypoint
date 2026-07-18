using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

public class ProgressQueryHandler : IRequestHandler<ProgressQuery, ProgressView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ProgressQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProgressView> Handle(ProgressQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var trackLessonIds = await TrackLessonIdsAsync(youthId, cancellationToken);
        var completedLessonIds = await _db.LessonProgress.AsNoTracking()
            .Where(item => item.YouthUserId == youthId
                && item.Status == ProgressStatus.Completed
                && trackLessonIds.Contains(item.LessonId))
            .Select(item => item.LessonId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var lessonsCompleted = completedLessonIds.Count;
        var overallPercent = trackLessonIds.Count == 0
            ? 0
            : (double)lessonsCompleted / trackLessonIds.Count * 100;

        var quizHistory = await QuizHistoryAsync(youthId, cancellationToken);
        var quizAverage = quizHistory.Count == 0 ? 0 : quizHistory.Average(item => item.Score);
        var attendanceHistory = await AttendanceHistoryAsync(youthId, cancellationToken);
        var rateRecords = attendanceHistory
            .Where(item => item.Status != AttendanceStatus.Excused.ToString())
            .ToList();
        var attended = rateRecords.Count(item => item.Status is "Present" or "Late");
        var attendanceRate = rateRecords.Count == 0 ? 0 : (double)attended / rateRecords.Count * 100;
        var competencies = await CompetenciesAsync(youthId, cancellationToken);

        return new ProgressView(
            overallPercent,
            quizAverage,
            lessonsCompleted,
            trackLessonIds.Count,
            attendanceRate,
            quizHistory,
            attendanceHistory,
            competencies);
    }

    private async Task<List<Guid>> TrackLessonIdsAsync(Guid youthId, CancellationToken cancellationToken)
    {
        var trackId = await CurriculumProgressCalculator.ResolveYouthTrackIdAsync(
            _db, youthId, cancellationToken);
        if (trackId is not { } selectedTrackId) return [];
        var moduleIds = await _db.Modules.AsNoTracking()
            .Where(item => item.TrackId == selectedTrackId && item.Status == PublishStatus.Published)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        return await _db.Lessons.AsNoTracking()
            .Where(item => moduleIds.Contains(item.ModuleId)
                && item.Status == PublishStatus.Published
                && item.ArchivedAt == null)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<QuizHistoryItem>> QuizHistoryAsync(Guid youthId, CancellationToken cancellationToken)
    {
        var attempts = await _db.QuizAttempts.AsNoTracking()
            .Where(item => item.YouthUserId == youthId
                && item.IsSubmitted
                && item.ApprovedScore != null
                && item.ApprovedAt != null)
            .OrderByDescending(item => item.ApprovedAt)
            .ToListAsync(cancellationToken);
        var quizIds = attempts.Select(item => item.QuizId).Distinct().ToList();
        var quizzes = await _db.Quizzes.AsNoTracking()
            .Where(item => quizIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
        var moduleIds = quizzes
            .Where(item => item.ModuleId != null)
            .Select(item => item.ModuleId!.Value)
            .Distinct()
            .ToList();
        var lessonIds = quizzes
            .Where(item => item.LessonId != null)
            .Select(item => item.LessonId!.Value)
            .Distinct()
            .ToList();
        var lessons = await _db.Lessons.AsNoTracking()
            .Where(item => lessonIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        moduleIds.AddRange(lessons.Values.Select(item => item.ModuleId));
        moduleIds = moduleIds.Distinct().ToList();
        var modules = await _db.Modules.AsNoTracking()
            .Where(item => moduleIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var result = new List<QuizHistoryItem>();
        foreach (var attempt in attempts)
        {
            var quiz = quizzes.FirstOrDefault(item => item.Id == attempt.QuizId);
            if (quiz is null) continue;
            var lesson = quiz.LessonId is { } lessonId ? lessons.GetValueOrDefault(lessonId) : null;
            var moduleId = quiz.ModuleId ?? lesson?.ModuleId;
            var moduleName = moduleId is { } resolvedModuleId
                ? modules.GetValueOrDefault(resolvedModuleId, "Learning module")
                : "Learning module";
            var quizTitle = lesson is null ? $"{moduleName} quiz" : $"{lesson.Title} quiz";
            var score = attempt.ApprovedScore!.Value;
            result.Add(new(
                attempt.Id,
                quiz.Id,
                quizTitle,
                moduleName,
                score,
                score >= quiz.PassMark,
                attempt.ApprovedAt!.Value));
        }
        return result;
    }

    private async Task<List<AttendanceHistoryItem>> AttendanceHistoryAsync(
        Guid youthId,
        CancellationToken cancellationToken) =>
        await (
            from record in _db.AttendanceRecords.AsNoTracking()
            join session in _db.Sessions.AsNoTracking() on record.SessionId equals session.Id
            where record.YouthUserId == youthId
            orderby session.StartsAt descending
            select new AttendanceHistoryItem(
                session.Id,
                session.Title,
                session.StartsAt,
                record.Status.ToString()))
            .ToListAsync(cancellationToken);

    private async Task<List<CompetencyLevelDto>> CompetenciesAsync(
        Guid youthId,
        CancellationToken cancellationToken)
    {
        var assessments = await _db.CompetencyAssessments.AsNoTracking()
            .Where(item => item.YouthUserId == youthId)
            .ToListAsync(cancellationToken);
        var latest = assessments
            .GroupBy(item => item.CompetencyId)
            .Select(group => group.OrderByDescending(item => item.AssessedAt).First())
            .ToList();
        var ids = latest.Select(item => item.CompetencyId).ToList();
        var definitions = await _db.Competencies.AsNoTracking()
            .Where(item => ids.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        return latest
            .Where(item => definitions.ContainsKey(item.CompetencyId))
            .Select(item => new CompetencyLevelDto(
                item.CompetencyId,
                definitions[item.CompetencyId],
                item.Level.ToString(),
                PercentFor(item.Level),
                item.AssessedAt))
            .OrderBy(item => item.Name)
            .ToList();
    }

    private static int PercentFor(CompetencyLevel level) => level switch
    {
        CompetencyLevel.Developing => 40,
        CompetencyLevel.Proficient => 75,
        CompetencyLevel.Mastered => 100,
        _ => 0,
    };
}

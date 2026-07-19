using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ReportingAndData.Reports;

public record YouthReportQuery(Guid YouthId, ReportFilter? Filter = null) : IRequest<YouthReport>;

public record QuizScoreReport(Guid QuizId, string Title, int Score, DateTime? SubmittedAt);
public record CompetencyReport(Guid CompetencyId, string Name, string Level, DateTime AssessedAt);
public record MentorNotesSummary(int CheckIns, DateTime? LatestAt, string? LatestEngagement);
public record BadgeReport(Guid BadgeId, string Name, DateTime AwardedAt);

public record YouthReport(
    Guid YouthId,
    string YouthName,
    string? CohortName,
    string? MentorName,
    int LessonsCompleted,
    int LessonsTotal,
    double CompletionRate,
    double AvgQuizScore,
    int AttendancePresent,
    int AttendanceTotal,
    int Badges,
    AttendanceBreakdown Attendance,
    IReadOnlyList<ModuleReport> Modules,
    IReadOnlyList<QuizScoreReport> QuizScores,
    IReadOnlyList<CompetencyReport> Competencies,
    MentorNotesSummary MentorNotes,
    IReadOnlyList<BadgeReport> BadgeAwards);

public class YouthReportQueryHandler : IRequestHandler<YouthReportQuery, YouthReport>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public YouthReportQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<YouthReport> Handle(YouthReportQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new ReportFilter();
        var youth = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.YouthId && item.Role == Roles.Youth, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
            throw new ForbiddenException("This youth is not assigned to you.");

        var end = ReportData.EndExclusive(filter.To);
        var lessonsCompleted = await _db.LessonProgress.AsNoTracking()
            .CountAsync(item => item.YouthUserId == youth.Id
                && item.Status == ProgressStatus.Completed
                && (filter.From == null || item.CompletedAt >= filter.From)
                && (end == null || item.CompletedAt < end), cancellationToken);
        var modules = await ReportData.ModulesForYouthAsync(_db, youth, filter, cancellationToken);
        var lessonsTotal = modules.Sum(item => item.TotalLessons);
        var completionRate = lessonsTotal == 0 ? 0d : modules.Sum(item => item.CompletedLessons) / (double)lessonsTotal;

        var attemptsQuery = _db.QuizAttempts.AsNoTracking()
            .Where(item => item.YouthUserId == youth.Id
                && item.IsSubmitted
                && (filter.From == null || item.SubmittedAt >= filter.From)
                && (end == null || item.SubmittedAt < end));
        if (filter.TrackId is Guid trackId)
        {
            var moduleIds = await _db.Modules.AsNoTracking()
                .Where(item => item.TrackId == trackId)
                .Select(item => item.Id)
                .ToListAsync(cancellationToken);
            var lessonIds = await _db.Lessons.AsNoTracking()
                .Where(item => moduleIds.Contains(item.ModuleId))
                .Select(item => item.Id)
                .ToListAsync(cancellationToken);
            var quizIds = await _db.Quizzes.AsNoTracking()
                .Where(item => (item.ModuleId != null && moduleIds.Contains(item.ModuleId.Value))
                    || (item.LessonId != null && lessonIds.Contains(item.LessonId.Value)))
                .Select(item => item.Id)
                .ToListAsync(cancellationToken);
            attemptsQuery = attemptsQuery.Where(item => quizIds.Contains(item.QuizId));
        }
        var attempts = await attemptsQuery.OrderByDescending(item => item.SubmittedAt).ToListAsync(cancellationToken);
        var quizIdsForTitles = attempts.Select(item => item.QuizId).Distinct().ToList();
        var quizzes = await _db.Quizzes.AsNoTracking()
            .Where(item => quizIdsForTitles.Contains(item.Id))
            .ToListAsync(cancellationToken);
        var moduleIdsForTitles = quizzes.Where(item => item.ModuleId != null)
            .Select(item => item.ModuleId!.Value)
            .Distinct()
            .ToList();
        var lessonIdsForTitles = quizzes.Where(item => item.LessonId != null)
            .Select(item => item.LessonId!.Value)
            .Distinct()
            .ToList();
        var moduleNames = await _db.Modules.AsNoTracking()
            .Where(item => moduleIdsForTitles.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var lessonNames = await _db.Lessons.AsNoTracking()
            .Where(item => lessonIdsForTitles.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Title, cancellationToken);
        var quizMap = quizzes.ToDictionary(item => item.Id);
        var quizScores = attempts.Select(item =>
        {
            quizMap.TryGetValue(item.QuizId, out var quiz);
            var title = quiz?.LessonId is Guid lessonId
                ? $"{lessonNames.GetValueOrDefault(lessonId, "Quiz")} quiz"
                : quiz?.ModuleId is Guid moduleId
                    ? $"{moduleNames.GetValueOrDefault(moduleId, "Module")} quiz"
                    : "Quiz";
            return new QuizScoreReport(
                item.QuizId,
                title,
                item.ApprovedScore ?? item.AutoScore,
                item.SubmittedAt);
        }).ToList();
        var avgQuizScore = quizScores.Count == 0 ? 0d : quizScores.Average(item => item.Score);

        var attendance = await ReportData.AttendanceAsync(_db, [youth.Id], filter, cancellationToken);
        var assessments = await _db.CompetencyAssessments.AsNoTracking()
            .Where(item => item.YouthUserId == youth.Id
                && (filter.From == null || item.AssessedAt >= filter.From)
                && (end == null || item.AssessedAt < end))
            .OrderByDescending(item => item.AssessedAt)
            .ToListAsync(cancellationToken);
        var latestAssessments = assessments.GroupBy(item => item.CompetencyId)
            .Select(group => group.First())
            .ToList();
        var competencyIds = latestAssessments.Select(item => item.CompetencyId).ToList();
        var competencyNames = await _db.Competencies.AsNoTracking()
            .Where(item => competencyIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var competencies = latestAssessments.Select(item => new CompetencyReport(
            item.CompetencyId,
            competencyNames.GetValueOrDefault(item.CompetencyId, "Competency"),
            item.Level.ToString(),
            item.AssessedAt)).ToList();

        var checkIns = await _db.CheckIns.AsNoTracking()
            .Where(item => item.YouthUserId == youth.Id
                && (filter.From == null || item.Date >= filter.From)
                && (end == null || item.Date < end))
            .OrderByDescending(item => item.Date)
            .Select(item => new { item.Date, item.Engagement })
            .ToListAsync(cancellationToken);
        var mentorNotes = new MentorNotesSummary(
            checkIns.Count,
            checkIns.FirstOrDefault()?.Date,
            checkIns.FirstOrDefault()?.Engagement);

        var badgeAwards = await (
            from award in _db.BadgeAwards.AsNoTracking()
            join badge in _db.Badges.AsNoTracking() on award.BadgeId equals badge.Id
            where award.YouthUserId == youth.Id
                && (filter.From == null || award.AwardedAt >= filter.From)
                && (end == null || award.AwardedAt < end)
            orderby award.AwardedAt descending
            select new BadgeReport(badge.Id, badge.Name, award.AwardedAt))
            .ToListAsync(cancellationToken);
        var badgeCount = await _db.BadgeAwards.AsNoTracking()
            .CountAsync(item => item.YouthUserId == youth.Id
                && (filter.From == null || item.AwardedAt >= filter.From)
                && (end == null || item.AwardedAt < end), cancellationToken);
        var cohortName = youth.CohortId is Guid cohortId
            ? await _db.Cohorts.AsNoTracking().Where(item => item.Id == cohortId)
                .Select(item => item.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        var mentorName = youth.MentorId is Guid mentorId
            ? await _db.Users.AsNoTracking().Where(item => item.Id == mentorId)
                .Select(item => item.DisplayName).FirstOrDefaultAsync(cancellationToken)
            : null;

        return new YouthReport(
            youth.Id,
            youth.DisplayName,
            cohortName,
            mentorName,
            lessonsCompleted,
            lessonsTotal,
            completionRate,
            avgQuizScore,
            attendance.Present + attendance.Late,
            attendance.Total,
            badgeCount,
            attendance,
            modules,
            quizScores,
            competencies,
            mentorNotes,
            badgeAwards);
    }
}

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

        var quizHistory = await ProgressEvidenceCalculator.QuizHistoryAsync(
            _db, youthId, cancellationToken);
        var quizAverage = quizHistory.Count == 0 ? 0 : quizHistory.Average(item => item.Score);
        var attendanceHistory = await AttendanceHistoryAsync(youthId, cancellationToken);
        var rateRecords = attendanceHistory
            .Where(item => item.Status != AttendanceStatus.Excused.ToString())
            .ToList();
        var attended = rateRecords.Count(item => item.Status is "Present" or "Late");
        var attendanceRate = rateRecords.Count == 0 ? 0 : (double)attended / rateRecords.Count * 100;
        var competencies = await ProgressEvidenceCalculator.CompetenciesAsync(
            _db, youthId, cancellationToken);
        var sharedCheckIns = await _db.CheckIns.AsNoTracking()
            .Where(checkIn => checkIn.YouthUserId == youthId
                && checkIn.Visibility == Visibility.SharedWithMentor)
            .OrderByDescending(checkIn => checkIn.Date)
            .Select(checkIn => new YouthSharedCheckIn(
                checkIn.Id,
                checkIn.Date,
                checkIn.Type,
                checkIn.Engagement,
                checkIn.GoalsDiscussed,
                checkIn.PrayerRequests,
                checkIn.ActionItems,
                checkIn.NextFollowUp))
            .ToListAsync(cancellationToken);

        return new ProgressView(
            overallPercent,
            quizAverage,
            lessonsCompleted,
            trackLessonIds.Count,
            attendanceRate,
            quizHistory,
            attendanceHistory,
            competencies,
            sharedCheckIns);
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

}

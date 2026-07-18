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

        // Total published lessons in the youth's track (the denominator for overall progress).
        var trackLessonIds = new List<Guid>();
        var trackId = await CurriculumProgressCalculator.ResolveYouthTrackIdAsync(_db, youthId, cancellationToken);
        if (trackId is { } tid)
        {
            var trackModuleIds = await _db.Modules
                .Where(m => m.TrackId == tid && m.Status == PublishStatus.Published)
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);

            trackLessonIds = await _db.Lessons
                .Where(l => trackModuleIds.Contains(l.ModuleId)
                    && l.Status == PublishStatus.Published
                    && l.ArchivedAt == null)
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);
        }

        var completedLessonIds = await _db.LessonProgress
            .Where(p => p.YouthUserId == youthId && p.Status == ProgressStatus.Completed)
            .Select(p => p.LessonId)
            .ToListAsync(cancellationToken);

        var lessonsCompleted = completedLessonIds.Count;

        var completedInTrack = trackLessonIds.Count == 0
            ? 0
            : completedLessonIds.Count(id => trackLessonIds.Contains(id));
        var overallPercent = trackLessonIds.Count == 0
            ? 0
            : (double)completedInTrack / trackLessonIds.Count * 100;

        // Quiz average uses mentor-approved scores only.
        var approvedScores = await _db.QuizAttempts
            .Where(a => a.YouthUserId == youthId && a.ApprovedScore != null)
            .Select(a => a.ApprovedScore!.Value)
            .ToListAsync(cancellationToken);
        var quizAverage = approvedScores.Count == 0 ? 0 : approvedScores.Average();

        // Latest assessment per competency for this youth.
        var assessments = await _db.CompetencyAssessments
            .Where(a => a.YouthUserId == youthId)
            .ToListAsync(cancellationToken);
        var competencies = assessments
            .GroupBy(a => a.CompetencyId)
            .Select(g => g.OrderByDescending(a => a.AssessedAt).First())
            .Select(a => new CompetencyLevelDto(a.CompetencyId, a.Level.ToString()))
            .ToList();

        return new ProgressView(
            overallPercent,
            quizAverage,
            lessonsCompleted,
            AttendanceRate: 0, // ENGAGE/M4 — no attendance data yet.
            competencies);
    }
}

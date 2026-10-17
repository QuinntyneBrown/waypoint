using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed class GetTrackWorkspaceQueryHandler
    : IRequestHandler<GetTrackWorkspaceQuery, TrackWorkspaceView>
{
    private readonly IAppDbContext _db;

    public GetTrackWorkspaceQueryHandler(IAppDbContext db) => _db = db;

    public async Task<TrackWorkspaceView> Handle(
        GetTrackWorkspaceQuery request,
        CancellationToken cancellationToken)
    {
        var track = await _db.Tracks
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.TrackId, cancellationToken)
            ?? throw new NotFoundException("Track not found.");
        var modules = await _db.Modules
            .AsNoTracking()
            .Where(item => item.TrackId == track.Id)
            .OrderBy(item => item.Order)
            .ThenBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var moduleIds = modules.Select(item => item.Id).ToList();
        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(item => moduleIds.Contains(item.ModuleId) && item.ArchivedAt == null)
            .OrderBy(item => item.Order)
            .ThenBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var cohortNames = await _db.Cohorts
            .AsNoTracking()
            .Where(item => item.TrackId == track.Id)
            .OrderBy(item => item.Name)
            .Select(item => item.Name)
            .ToListAsync(cancellationToken);
        var lessonIds = lessons.Select(item => item.Id).ToList();
        var media = await _db.LessonMedia
            .AsNoTracking()
            .Where(item => lessonIds.Contains(item.LessonId))
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var quizzes = await _db.Quizzes
            .AsNoTracking()
            .Where(item => (item.ModuleId != null && moduleIds.Contains(item.ModuleId.Value))
                || (item.LessonId != null && lessonIds.Contains(item.LessonId.Value)))
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var quizIds = quizzes.Select(item => item.Id).ToList();
        var questionCounts = await _db.QuizQuestions
            .AsNoTracking()
            .Where(item => quizIds.Contains(item.QuizId))
            .GroupBy(item => item.QuizId)
            .Select(group => new { QuizId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.QuizId, item => item.Count, cancellationToken);
        var assignments = await _db.Assignments
            .AsNoTracking()
            .Where(item => (item.ModuleId != null && moduleIds.Contains(item.ModuleId.Value))
                || (item.LessonId != null && lessonIds.Contains(item.LessonId.Value)))
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var assignmentIds = assignments.Select(item => item.Id).ToList();
        var rubricTotals = await _db.RubricCriteria
            .AsNoTracking()
            .Where(item => assignmentIds.Contains(item.AssignmentId))
            .GroupBy(item => item.AssignmentId)
            .Select(group => new { AssignmentId = group.Key, Total = group.Sum(item => item.Points) })
            .ToDictionaryAsync(item => item.AssignmentId, item => item.Total, cancellationToken);

        return new TrackWorkspaceView(
            track.Id,
            track.Name,
            track.Key,
            track.Description,
            track.Status.ToString(),
            track.Enabled,
            cohortNames,
            modules.Select(module => new ModuleWorkspaceView(
                module.Id,
                module.Name,
                module.Description,
                module.Order,
                module.PrerequisiteModuleId,
                module.ScriptureAnchor,
                module.Status.ToString(),
                lessons
                    .Where(lesson => lesson.ModuleId == module.Id)
                    .Select(lesson => new LessonWorkspaceView(
                        lesson.Id,
                        lesson.Title,
                        lesson.Order,
                        lesson.Status.ToString(),
                        lesson.ScriptureReference,
                        lesson.EstimatedMinutes,
                        lesson.LearningObjectives,
                        lesson.TeachingBody,
                        lesson.ReflectionPrompts,
                        lesson.LeaderGuideNotes,
                        media
                            .Where(item => item.LessonId == lesson.Id)
                            .Select(item => new LessonMediaWorkspaceView(
                                item.Id,
                                item.MediaType,
                                item.OriginalFileName ?? "External media",
                                item.ContentType ?? string.Empty,
                                item.SizeBytes,
                                item.Url,
                                item.VideoStatus.ToString(),
                                item.ProgressPercent,
                                item.DurationSeconds,
                                item.FailureCode))
                            .ToList(),
                        quizzes
                            .Where(item => item.LessonId == lesson.Id)
                            .Select(item => ToQuizView(item, questionCounts))
                            .ToList(),
                        assignments
                            .Where(item => item.LessonId == lesson.Id)
                            .Select(item => ToAssignmentView(item, rubricTotals))
                            .ToList()))
                    .ToList(),
                quizzes
                    .Where(item => item.ModuleId == module.Id && item.LessonId == null)
                    .Select(item => ToQuizView(item, questionCounts))
                    .ToList(),
                assignments
                    .Where(item => item.ModuleId == module.Id && item.LessonId == null)
                    .Select(item => ToAssignmentView(item, rubricTotals))
                    .ToList()))
                .ToList());
    }

    private static QuizWorkspaceView ToQuizView(
        Quiz item,
        IReadOnlyDictionary<Guid, int> questionCounts) =>
        new(
            item.Id,
            item.ModuleId,
            item.LessonId,
            item.PassMark,
            item.AttemptsAllowed,
            item.FeedbackMode,
            item.Shuffle,
            item.RequiredToCompleteModule,
            questionCounts.GetValueOrDefault(item.Id));

    private static AssignmentWorkspaceView ToAssignmentView(
        Assignment item,
        IReadOnlyDictionary<Guid, int> rubricTotals) =>
        new(
            item.Id,
            item.ModuleId,
            item.LessonId,
            item.Title,
            item.TaskDescription,
            item.SubmissionTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            item.DueDate,
            item.GracePeriodDays,
            item.AllowLate,
            item.RequireMentorApproval,
            rubricTotals.GetValueOrDefault(item.Id, item.TotalPoints),
            item.Status.ToString());
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

/// <summary>Authoritative selected-track progress matrix (L2-ADMIN-004).</summary>
public sealed class ProgressBoardQueryHandler : IRequestHandler<ProgressBoardQuery, ProgressBoardResult>
{
    private const string Done = "Done";
    private const string InProgress = "InProgress";
    private const string NotStarted = "NotStarted";
    private const string NotAssigned = "NotAssigned";
    private readonly IAppDbContext _db;

    public ProgressBoardQueryHandler(IAppDbContext db) => _db = db;

    public async Task<ProgressBoardResult> Handle(
        ProgressBoardQuery request,
        CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.CohortId, cancellationToken)
            ?? throw new NotFoundException("Cohort not found.");
        var trackId = request.TrackId ?? cohort.TrackId
            ?? throw new ConflictException("Choose a curriculum track for this progress board.");
        var track = await _db.Tracks
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == trackId
                && item.Enabled
                && item.Status == PublishStatus.Published,
                cancellationToken)
            ?? throw new ConflictException("Choose an enabled, published curriculum track.");

        var moduleRows = await _db.Modules
            .AsNoTracking()
            .Where(item => item.TrackId == track.Id && item.Status == PublishStatus.Published)
            .OrderBy(item => item.Order)
            .Select(item => new { item.Id, item.Name })
            .ToListAsync(cancellationToken);
        var moduleIds = moduleRows.Select(item => item.Id).ToList();
        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(item => moduleIds.Contains(item.ModuleId)
                && item.Status == PublishStatus.Published
                && item.ArchivedAt == null)
            .OrderBy(item => item.Order)
            .Select(item => new { item.Id, item.ModuleId })
            .ToListAsync(cancellationToken);
        var lessonIds = lessons.Select(item => item.Id).ToList();
        var lessonsByModule = lessons
            .GroupBy(item => item.ModuleId)
            .ToDictionary(group => group.Key, group => group.Select(item => item.Id).ToList());

        var youth = await _db.Users
            .AsNoTracking()
            .Where(item => item.CohortId == cohort.Id
                && item.Role == Roles.Youth
                && item.Status != UserStatus.Removed)
            .OrderBy(item => item.DisplayName)
            .Select(item => new { item.Id, item.DisplayName })
            .ToListAsync(cancellationToken);
        var youthIds = youth.Select(item => item.Id).ToList();
        var explicitlyEnrolled = await _db.YouthTrackEnrollments
            .AsNoTracking()
            .Where(item => youthIds.Contains(item.YouthUserId) && item.TrackId == track.Id)
            .Select(item => item.YouthUserId)
            .ToListAsync(cancellationToken);
        var enrolledIds = explicitlyEnrolled.ToHashSet();
        if (cohort.TrackId == track.Id)
        {
            enrolledIds.UnionWith(youthIds);
        }

        var progressRows = await _db.LessonProgress
            .AsNoTracking()
            .Where(item => youthIds.Contains(item.YouthUserId) && lessonIds.Contains(item.LessonId))
            .Select(item => new { item.YouthUserId, item.LessonId, item.Status })
            .ToListAsync(cancellationToken);
        var progressByYouth = progressRows
            .GroupBy(item => item.YouthUserId)
            .ToDictionary(
                group => group.Key,
                group => group.ToDictionary(item => item.LessonId, item => item.Status));

        var quizIds = await _db.Quizzes
            .AsNoTracking()
            .Where(item => (item.ModuleId.HasValue && moduleIds.Contains(item.ModuleId.Value))
                || (item.LessonId.HasValue && lessonIds.Contains(item.LessonId.Value)))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var approvedAttempts = await _db.QuizAttempts
            .AsNoTracking()
            .Where(item => youthIds.Contains(item.YouthUserId)
                && quizIds.Contains(item.QuizId)
                && item.ApprovedScore.HasValue)
            .Select(item => new { item.YouthUserId, item.QuizId, Score = item.ApprovedScore!.Value })
            .ToListAsync(cancellationToken);
        var quizAverageByYouth = approvedAttempts
            .GroupBy(item => item.YouthUserId)
            .ToDictionary(
                youthGroup => youthGroup.Key,
                youthGroup => youthGroup
                    .GroupBy(item => item.QuizId)
                    .Select(quizGroup => quizGroup.Max(item => item.Score))
                    .Average());

        var rows = new List<YouthRow>();
        foreach (var person in youth)
        {
            var assigned = enrolledIds.Contains(person.Id);
            var personProgress = progressByYouth.GetValueOrDefault(person.Id)
                ?? new Dictionary<Guid, ProgressStatus>();
            var cells = new List<CellState>();
            foreach (var module in moduleRows)
            {
                var moduleLessonIds = lessonsByModule.GetValueOrDefault(module.Id) ?? [];
                cells.Add(BuildCell(module.Id, moduleLessonIds, personProgress, assigned));
            }

            var completedLessons = assigned
                ? lessonIds.Count(id => personProgress.GetValueOrDefault(id) == ProgressStatus.Completed)
                : 0;
            var overall = assigned && lessonIds.Count > 0
                ? (double)completedLessons / lessonIds.Count
                : 0;
            rows.Add(new YouthRow(
                person.Id,
                person.DisplayName,
                cells,
                quizAverageByYouth.GetValueOrDefault(person.Id),
                overall));
        }

        var modules = moduleRows.Select((module, index) =>
        {
            var assignedCells = rows
                .Select(row => row.Cells[index])
                .Where(cell => cell.State != NotAssigned)
                .ToList();
            var average = assignedCells.Count == 0 ? 0 : assignedCells.Average(cell => cell.Percent);
            return new ModuleColumn(module.Id, module.Name, average);
        }).ToList();
        var assignedRows = rows.Where(row => row.Cells.Any(cell => cell.State != NotAssigned)).ToList();
        var cohortAverage = assignedRows.Count == 0 ? 0 : assignedRows.Average(row => row.OverallPercent);

        return new ProgressBoardResult(
            cohort.Id,
            cohort.Name,
            track.Id,
            track.Name,
            modules,
            rows,
            cohortAverage);
    }

    private static CellState BuildCell(
        Guid moduleId,
        IReadOnlyList<Guid> lessonIds,
        IReadOnlyDictionary<Guid, ProgressStatus> progress,
        bool assigned)
    {
        if (!assigned)
        {
            return new CellState(moduleId, NotAssigned, 0);
        }
        if (lessonIds.Count == 0)
        {
            return new CellState(moduleId, NotStarted, 0);
        }

        var completed = lessonIds.Count(id => progress.GetValueOrDefault(id) == ProgressStatus.Completed);
        var percent = (double)completed / lessonIds.Count;
        if (completed == lessonIds.Count)
        {
            return new CellState(moduleId, Done, percent);
        }
        var started = lessonIds.Any(id =>
            progress.TryGetValue(id, out var status) && status != ProgressStatus.NotStarted);
        return new CellState(moduleId, started ? InProgress : NotStarted, percent);
    }
}

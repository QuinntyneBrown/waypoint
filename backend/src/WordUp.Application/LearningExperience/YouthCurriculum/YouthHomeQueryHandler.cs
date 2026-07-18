using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

public sealed class YouthHomeQueryHandler : IRequestHandler<YouthHomeQuery, YouthHomeView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public YouthHomeQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<YouthHomeView> Handle(YouthHomeQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var placement = await _db.Users.AsNoTracking()
            .Where(item => item.Id == youthId)
            .Select(item => new { item.CohortId, item.MentorId })
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("User not found.");
        if (placement.CohortId is not { } cohortId)
            return new YouthHomeView(false, null, null, 0, 0, 0, null, null, []);

        var cohort = await _db.Cohorts.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == cohortId, cancellationToken);
        if (cohort is null)
            return new YouthHomeView(false, null, null, 0, 0, 0, null, null, []);
        var mentorName = placement.MentorId is { } mentorId
            ? await _db.Users.AsNoTracking()
                .Where(item => item.Id == mentorId && item.Role == Roles.Mentor)
                .Select(item => item.DisplayName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        IReadOnlyList<ModuleStatus> modules = [];
        if (cohort.TrackId is { } trackId && await _db.Tracks.AsNoTracking().AnyAsync(
            item => item.Id == trackId && item.Enabled && item.Status == PublishStatus.Published,
            cancellationToken))
        {
            modules = await CurriculumProgressCalculator.BuildModuleStatusesAsync(
                _db, trackId, youthId, cancellationToken);
        }
        var completedItems = modules.Sum(item => item.CompletedItems);
        var totalItems = modules.Sum(item => item.TotalItems);
        var overallPercent = totalItems == 0 ? 0 : (double)completedItems / totalItems * 100;
        var currentModule = modules.FirstOrDefault(item => item.Current)
            ?? modules.FirstOrDefault(item => !item.Locked && item.Status != "done");
        var currentItem = currentModule?.Items.FirstOrDefault(item => !item.Locked && item.Status != "done");
        var continueLearning = currentModule is null || currentItem is null
            ? null
            : new HomeLearningItem(
                currentItem.ItemId,
                currentItem.Type,
                currentItem.Title,
                currentModule.Name,
                currentItem.Status,
                currentItem.EstimatedMinutes,
                LearningPath(currentItem));

        var nextSessionEntity = await _db.Sessions.AsNoTracking()
            .Where(item => item.CohortId == cohortId && item.StartsAt > _clock.UtcNow)
            .OrderBy(item => item.StartsAt)
            .FirstOrDefaultAsync(cancellationToken);
        HomeSession? nextSession = null;
        if (nextSessionEntity is not null)
        {
            var response = await _db.SessionRsvps.AsNoTracking()
                .Where(item => item.SessionId == nextSessionEntity.Id && item.YouthUserId == youthId)
                .Select(item => (RsvpResponse?)item.Response)
                .FirstOrDefaultAsync(cancellationToken);
            nextSession = new HomeSession(
                nextSessionEntity.Id,
                nextSessionEntity.Title,
                nextSessionEntity.StartsAt,
                nextSessionEntity.DurationMinutes,
                nextSessionEntity.Location,
                response?.ToString());
        }

        var assignedWork = await ResolveAssignedWorkAsync(youthId, modules, cancellationToken);
        return new YouthHomeView(
            true,
            cohort.Name,
            mentorName,
            completedItems,
            totalItems,
            overallPercent,
            continueLearning,
            nextSession,
            assignedWork);
    }

    private async Task<IReadOnlyList<HomeAssignedWork>> ResolveAssignedWorkAsync(
        Guid youthId,
        IReadOnlyList<ModuleStatus> modules,
        CancellationToken cancellationToken)
    {
        var work = await _db.AssignedWork.AsNoTracking()
            .Where(item => item.YouthUserId == youthId)
            .OrderBy(item => item.DueDate == null)
            .ThenBy(item => item.DueDate)
            .ThenByDescending(item => item.CreatedAt)
            .Take(6)
            .ToListAsync(cancellationToken);
        var lessonIds = work.Where(item => item.ItemType.ToLower() == "lesson")
            .Select(item => item.ItemId).ToList();
        var moduleIds = work.Where(item => item.ItemType.ToLower() == "module")
            .Select(item => item.ItemId).ToList();
        var projectIds = work.Where(item => item.ItemType.ToLower() is "project" or "assignment")
            .Select(item => item.ItemId).ToList();
        var lessons = await _db.Lessons.AsNoTracking()
            .Where(item => lessonIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var moduleNames = await _db.Modules.AsNoTracking()
            .Where(item => moduleIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var assignments = await _db.Assignments.AsNoTracking()
            .Where(item => projectIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var completedLessons = await _db.LessonProgress.AsNoTracking()
            .Where(item => item.YouthUserId == youthId
                && lessonIds.Contains(item.LessonId)
                && item.Status == ProgressStatus.Completed)
            .Select(item => item.LessonId)
            .ToListAsync(cancellationToken);
        var submissions = await _db.Submissions.AsNoTracking()
            .Where(item => item.YouthUserId == youthId && projectIds.Contains(item.AssignmentId))
            .OrderByDescending(item => item.SubmittedAt)
            .ToListAsync(cancellationToken);

        var result = new List<HomeAssignedWork>();
        foreach (var item in work)
        {
            var itemType = item.ItemType.ToLowerInvariant();
            string? title = null;
            string? path = null;
            var status = "Assigned";
            if (itemType == "lesson" && lessons.TryGetValue(item.ItemId, out var lesson))
            {
                title = lesson.Title;
                path = lesson.IsCodingLesson
                    ? $"/youth/coding/{lesson.Id}"
                    : $"/youth/lessons/{lesson.Id}";
                if (completedLessons.Contains(lesson.Id)) status = "Completed";
            }
            else if (itemType == "module" && moduleNames.TryGetValue(item.ItemId, out var moduleName))
            {
                title = moduleName;
                path = "/youth/curriculum";
                if (modules.Any(module => module.ModuleId == item.ItemId && module.Status == "done"))
                    status = "Completed";
            }
            else if (itemType is "project" or "assignment" && assignments.TryGetValue(item.ItemId, out var assignment))
            {
                title = assignment.Title;
                path = $"/youth/assignments/{assignment.Id}";
                var submission = submissions.FirstOrDefault(entry => entry.AssignmentId == assignment.Id);
                if (submission is not null)
                {
                    status = submission.Status.ToString();
                    path = $"/youth/submissions/{submission.Id}";
                }
            }

            if (title is null || path is null) continue;
            var completed = status is "Completed" or "Approved";
            result.Add(new HomeAssignedWork(
                item.Id,
                item.ItemId,
                item.ItemType,
                title,
                item.DueDate,
                status,
                !completed && item.DueDate is { } dueDate && dueDate < _clock.UtcNow,
                item.Message,
                path));
        }
        return result;
    }

    private static string LearningPath(CurriculumItem item) => item.Type switch
    {
        "coding" => $"/youth/coding/{item.ItemId}",
        "quiz" => $"/youth/quizzes/{item.ItemId}",
        _ => $"/youth/lessons/{item.ItemId}",
    };
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.AssignWork;

public sealed class AssignWorkOptionsQueryHandler : IRequestHandler<AssignWorkOptionsQuery, AssignWorkOptions>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AssignWorkOptionsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AssignWorkOptions> Handle(
        AssignWorkOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var youthQuery = _db.Users.Where(user =>
            user.Role == Roles.Youth && user.Status == UserStatus.Active);
        if (_currentUser.Role == Roles.Mentor)
        {
            youthQuery = youthQuery.Where(user => user.MentorId == userId);
        }
        var youth = await youthQuery
            .Select(user => new { user.Id, user.DisplayName, user.CohortId })
            .ToListAsync(cancellationToken);
        var youthIds = youth.Select(user => user.Id).ToList();
        var cohortIds = youth
            .Where(user => user.CohortId != null)
            .Select(user => user.CohortId!.Value)
            .Distinct()
            .ToList();
        var cohorts = await _db.Cohorts
            .Where(cohort => cohortIds.Contains(cohort.Id))
            .ToListAsync(cancellationToken);
        var cohortNames = cohorts.ToDictionary(cohort => cohort.Id, cohort => cohort.Name);
        var activeFlagIds = await _db.Flags
            .Where(flag => youthIds.Contains(flag.YouthUserId) && flag.ResolvedAt == null)
            .Select(flag => flag.YouthUserId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var activeFlags = activeFlagIds.ToHashSet();

        var allCohortCounts = await _db.Users
            .Where(user => user.CohortId != null
                && cohortIds.Contains(user.CohortId.Value)
                && user.Role == Roles.Youth
                && user.Status == UserStatus.Active)
            .GroupBy(user => user.CohortId!.Value)
            .Select(group => new { CohortId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.CohortId, row => row.Count, cancellationToken);
        var scopedCohortCounts = youth
            .Where(user => user.CohortId != null)
            .GroupBy(user => user.CohortId!.Value)
            .ToDictionary(group => group.Key, group => group.Count());
        var assignableCohorts = cohorts
            .Where(cohort => scopedCohortCounts.GetValueOrDefault(cohort.Id) > 0
                && (_currentUser.Role == Roles.ProgramAdministrator
                    || scopedCohortCounts.GetValueOrDefault(cohort.Id)
                        == allCohortCounts.GetValueOrDefault(cohort.Id)))
            .OrderBy(cohort => cohort.Name)
            .Select(cohort => new AssignableCohort(
                cohort.Id,
                cohort.Name,
                scopedCohortCounts.GetValueOrDefault(cohort.Id)))
            .ToList();

        var trackIds = cohorts
            .Where(cohort => cohort.TrackId != null)
            .Select(cohort => cohort.TrackId!.Value)
            .ToList();
        trackIds.AddRange(await _db.YouthTrackEnrollments
            .Where(enrollment => youthIds.Contains(enrollment.YouthUserId))
            .Select(enrollment => enrollment.TrackId)
            .ToListAsync(cancellationToken));
        trackIds = trackIds.Distinct().ToList();
        var enabledTrackIds = await _db.Tracks
            .Where(track => trackIds.Contains(track.Id)
                && track.Enabled
                && track.Status == PublishStatus.Published)
            .Select(track => track.Id)
            .ToListAsync(cancellationToken);
        var modules = await _db.Modules
            .Where(module => enabledTrackIds.Contains(module.TrackId)
                && module.Status == PublishStatus.Published)
            .OrderBy(module => module.Order)
            .ThenBy(module => module.Name)
            .ToListAsync(cancellationToken);
        var moduleIds = modules.Select(module => module.Id).ToList();
        var moduleNames = modules.ToDictionary(module => module.Id, module => module.Name);
        var lessons = await _db.Lessons
            .Where(lesson => moduleIds.Contains(lesson.ModuleId)
                && lesson.Status == PublishStatus.Published
                && lesson.ArchivedAt == null)
            .OrderBy(lesson => lesson.Order)
            .ThenBy(lesson => lesson.Title)
            .ToListAsync(cancellationToken);
        var lessonIds = lessons.Select(lesson => lesson.Id).ToList();
        var assignments = await _db.Assignments
            .Where(assignment =>
                (assignment.ModuleId != null && moduleIds.Contains(assignment.ModuleId.Value))
                || (assignment.LessonId != null && lessonIds.Contains(assignment.LessonId.Value)))
            .OrderBy(assignment => assignment.Title)
            .ToListAsync(cancellationToken);

        var items = modules
            .Select(module => new AssignableWorkItem(
                module.Id, AssignWorkItemTypes.Module, module.Name, module.Name))
            .Concat(lessons.Select(lesson => new AssignableWorkItem(
                lesson.Id,
                AssignWorkItemTypes.Lesson,
                lesson.Title,
                moduleNames.GetValueOrDefault(lesson.ModuleId, "Module"))))
            .Concat(assignments.Select(assignment =>
            {
                var moduleId = assignment.ModuleId
                    ?? lessons.FirstOrDefault(lesson => lesson.Id == assignment.LessonId)?.ModuleId;
                return new AssignableWorkItem(
                    assignment.Id,
                    AssignWorkItemTypes.Project,
                    assignment.Title,
                    moduleId is { } id ? moduleNames.GetValueOrDefault(id, "Module") : "Module");
            }))
            .OrderBy(item => item.ModuleName)
            .ThenBy(item => item.ItemType)
            .ThenBy(item => item.Title)
            .ToList();

        return new AssignWorkOptions(
            items,
            youth.OrderBy(user => user.DisplayName)
                .Select(user => new AssignableYouth(
                    user.Id,
                    user.DisplayName,
                    user.CohortId,
                    user.CohortId is { } cohortId
                        ? cohortNames.GetValueOrDefault(cohortId)
                        : null,
                    activeFlags.Contains(user.Id)))
                .ToList(),
            assignableCohorts);
    }
}

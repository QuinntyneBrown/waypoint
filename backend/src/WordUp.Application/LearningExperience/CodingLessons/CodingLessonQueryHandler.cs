using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.LearningExperience.YouthCurriculum;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.CodingLessons;

public sealed class CodingLessonQueryHandler : IRequestHandler<CodingLessonQuery, CodingLessonView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CodingLessonQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CodingLessonView> Handle(CodingLessonQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var access = await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
            _db, request.LessonId, youthId, cancellationToken);
        if (!access.Lesson.IsCodingLesson) throw new NotFoundException("Coding lesson not found.");

        var assignment = await _db.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(item => item.LessonId == access.Lesson.Id, cancellationToken)
            ?? throw new NotFoundException("Coding project not found.");
        var steps = await _db.CodingLessonSteps.AsNoTracking()
            .Where(item => item.LessonId == access.Lesson.Id)
            .OrderBy(item => item.Order)
            .Select(item => new CodingLessonStepView(
                item.Id, item.Order, item.Title, item.Instructions, item.CodeSample, item.ChecklistLabel))
            .ToListAsync(cancellationToken);
        var workspace = await _db.CodingLessonWorkspaces.AsNoTracking().FirstOrDefaultAsync(
            item => item.YouthUserId == youthId && item.LessonId == access.Lesson.Id,
            cancellationToken);
        var checkedIds = ParseIds(workspace?.CheckedStepIdsJson)
            .Where(id => steps.Any(step => step.Id == id))
            .ToList();
        var awardIds = await _db.BadgeAwards.AsNoTracking()
            .Where(item => item.YouthUserId == youthId)
            .Select(item => item.BadgeId)
            .ToListAsync(cancellationToken);
        var badge = await _db.Badges.AsNoTracking()
            .Where(item => item.ArchivedAt == null
                && item.AutoRule == "coding-checklist-completed"
                && (item.TargetRefId == access.Lesson.Id || item.TargetRefId == null))
            .OrderBy(item => item.CreatedAt)
            .Select(item => new CodingLessonBadgeView(
                item.Id,
                item.Name,
                item.Icon,
                checkedIds.Count,
                steps.Count,
                awardIds.Contains(item.Id)))
            .FirstOrDefaultAsync(cancellationToken);
        var submission = await _db.Submissions.AsNoTracking()
            .Where(item => item.YouthUserId == youthId && item.AssignmentId == assignment.Id)
            .Select(item => new { item.Id, item.Status })
            .FirstOrDefaultAsync(cancellationToken);
        var percent = steps.Count == 0 ? 0 : checkedIds.Count * 100 / steps.Count;

        return new CodingLessonView(
            access.Lesson.Id,
            assignment.Id,
            access.Lesson.Title,
            access.Lesson.TeachingBody,
            workspace?.Code ?? access.Lesson.StarterCode ?? string.Empty,
            steps,
            checkedIds,
            percent,
            workspace?.CompletedAt is not null,
            badge,
            submission?.Id,
            submission?.Status.ToString());
    }

    internal static IReadOnlyList<Guid> ParseIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

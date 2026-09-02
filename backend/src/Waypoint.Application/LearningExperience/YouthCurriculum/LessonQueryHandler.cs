using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.LearningExperience.YouthCurriculum;

public sealed class LessonQueryHandler : IRequestHandler<LessonQuery, LessonView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public LessonQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<LessonView> Handle(LessonQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var access = await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
            _db,
            request.LessonId,
            youthId,
            cancellationToken);
        var lesson = access.Lesson;

        var media = await _db.LessonMedia
            .Where(item => item.LessonId == lesson.Id)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new LessonMediaView(item.Id, item.MediaType, item.Url))
            .ToListAsync(cancellationToken);
        var readingState = await _db.LessonReadingStates.FirstOrDefaultAsync(
            item => item.YouthUserId == youthId && item.LessonId == lesson.Id,
            cancellationToken);
        var orderedItems = access.ModuleStatus.Items.OrderBy(item => item.Order).ToList();
        var stepNumber = orderedItems.FindIndex(item => item.ItemId == lesson.Id) + 1;

        // LeaderGuideNotes is deliberately absent from this youth-safe projection.
        return new LessonView(
            lesson.Id,
            access.Module.Id,
            access.Module.Name,
            lesson.Title,
            lesson.ScriptureReference,
            lesson.LearningObjectives,
            lesson.TeachingBody,
            lesson.ReflectionPrompts,
            lesson.EstimatedMinutes,
            stepNumber,
            orderedItems.Count,
            access.Item.Status,
            media,
            new LessonReadingStateView(
                readingState?.TextSize ?? "medium",
                readingState?.Bookmarked ?? false,
                readingState?.Position ?? 0));
    }
}

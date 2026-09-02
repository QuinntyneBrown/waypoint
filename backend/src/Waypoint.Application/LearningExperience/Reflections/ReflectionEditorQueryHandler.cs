using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.LearningExperience.YouthCurriculum;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.LearningExperience.Reflections;

public sealed class ReflectionEditorQueryHandler : IRequestHandler<ReflectionEditorQuery, ReflectionEditorView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReflectionEditorQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ReflectionEditorView> Handle(
        ReflectionEditorQuery request,
        CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var accessible = await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
            _db, request.LessonId, youthId, cancellationToken);
        var reflection = await _db.Reflections
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.YouthUserId == youthId && item.LessonId == request.LessonId,
                cancellationToken);

        return new ReflectionEditorView(
            accessible.Lesson.Id,
            accessible.Lesson.Title,
            accessible.Lesson.ReflectionPrompts,
            reflection is null
                ? null
                : new ReflectionEntryView(
                    reflection.Id,
                    reflection.Text,
                    reflection.Mood,
                    ReflectionSecondaryResponseSerializer.Deserialize(reflection.SecondaryJson),
                    reflection.Visibility == Visibility.SharedWithMentor,
                    reflection.IsDraft,
                    reflection.UpdatedAt));
    }
}

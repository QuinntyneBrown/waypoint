using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.Curriculum;

public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, LessonDetail>
{
    private readonly IAppDbContext _db;

    public GetLessonQueryHandler(IAppDbContext db) => _db = db;

    public async Task<LessonDetail> Handle(GetLessonQuery request, CancellationToken cancellationToken)
    {
        // Staff view: includes leader-guide notes. Archived lessons are treated as gone (L2-CURRIC-002 AC5).
        var lesson = await _db.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.ArchivedAt == null, cancellationToken);

        if (lesson is null)
        {
            throw new NotFoundException("Lesson not found.");
        }

        return new LessonDetail(
            lesson.Id,
            lesson.ModuleId,
            lesson.Title,
            lesson.ScriptureReference,
            lesson.EstimatedMinutes,
            lesson.LearningObjectives,
            lesson.TeachingBody,
            lesson.ReflectionPrompts,
            lesson.LeaderGuideNotes,
            lesson.Status.ToString());
    }
}

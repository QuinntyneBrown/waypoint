using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Curriculum;

public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, LessonResult>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CreateLessonCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<LessonResult> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        var module = await _db.Modules.FirstOrDefaultAsync(m => m.Id == request.ModuleId, cancellationToken);
        if (module is null)
        {
            throw new NotFoundException("Module not found.");
        }

        // Lessons append to the end of the module and persist the full authoring payload
        // including learning objectives and leader-guide notes (L2-CURRIC-002 AC1).
        var maxOrder = await _db.Lessons
            .Where(l => l.ModuleId == request.ModuleId)
            .Select(l => (int?)l.Order)
            .MaxAsync(cancellationToken) ?? 0;

        var lesson = new Lesson
        {
            ModuleId = request.ModuleId,
            Title = request.Title.Trim(),
            ScriptureReference = request.ScriptureReference,
            EstimatedMinutes = request.EstimatedMinutes,
            LearningObjectives = request.LearningObjectives,
            TeachingBody = request.TeachingBody,
            ReflectionPrompts = request.ReflectionPrompts,
            LeaderGuideNotes = request.LeaderGuideNotes,
            Order = maxOrder + 1,
            Status = PublishStatus.Draft,
        };

        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("LessonCreated", _currentUser.UserId, "Lesson", lesson.Id);

        return new LessonResult(lesson.Id);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public sealed class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public UpdateLessonCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _db.Lessons.FirstOrDefaultAsync(
            item => item.Id == request.LessonId && item.ArchivedAt == null,
            cancellationToken) ?? throw new NotFoundException("Lesson not found.");
        if (lesson.ModuleId != request.ModuleId)
        {
            throw new ConflictException("A lesson cannot be moved to another module while editing it.");
        }

        lesson.Title = request.Title.Trim();
        lesson.ScriptureReference = Clean(request.ScriptureReference);
        lesson.EstimatedMinutes = request.EstimatedMinutes;
        lesson.LearningObjectives = Clean(request.LearningObjectives);
        lesson.TeachingBody = Clean(request.TeachingBody);
        lesson.ReflectionPrompts = Clean(request.ReflectionPrompts);
        lesson.LeaderGuideNotes = Clean(request.LeaderGuideNotes);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "LessonUpdated",
            _currentUser.UserId,
            "Lesson",
            lesson.Id,
            cancellationToken: cancellationToken);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

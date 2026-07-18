using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.Curriculum;

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public DeleteLessonCommandHandler(IAppDbContext db, IClock clock, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _db.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.ArchivedAt == null, cancellationToken);
        if (lesson is null)
        {
            throw new NotFoundException("Lesson not found.");
        }

        // Soft-delete so progress and linked quizzes are retained (L2-CURRIC-002 AC5).
        lesson.ArchivedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("LessonArchived", _currentUser.UserId, "Lesson", lesson.Id);
    }
}

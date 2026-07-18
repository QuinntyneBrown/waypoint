using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public class PublishLessonCommandHandler : IRequestHandler<PublishLessonCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public PublishLessonCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(PublishLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _db.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.ArchivedAt == null, cancellationToken);
        if (lesson is null)
        {
            throw new NotFoundException("Lesson not found.");
        }

        // Move the lesson through its publish lifecycle (L2-CURRIC-002 AC4).
        lesson.Status = request.Status;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "LessonPublishStateChanged", _currentUser.UserId, "Lesson", lesson.Id, details: request.Status.ToString());
    }
}

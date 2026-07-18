using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Reflections;

public class SaveReflectionCommandHandler : IRequestHandler<SaveReflectionCommand, ReflectionResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public SaveReflectionCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
    }

    public async Task<ReflectionResult> Handle(SaveReflectionCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var now = _clock.UtcNow;

        // Upsert: one reflection per youth+lesson (an existing draft is updated in place).
        var reflection = await _db.Reflections
            .FirstOrDefaultAsync(r => r.YouthUserId == youthId && r.LessonId == request.LessonId, cancellationToken);

        if (reflection is null)
        {
            reflection = new ReflectionEntry
            {
                YouthUserId = youthId,
                LessonId = request.LessonId,
                CreatedAt = now,
            };
            _db.Reflections.Add(reflection);
        }

        reflection.Text = request.Text;
        reflection.Mood = request.Mood;
        reflection.Visibility = request.ShareWithMentor ? Visibility.SharedWithMentor : Visibility.Private;
        reflection.IsDraft = request.IsDraft;
        reflection.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ReflectionSaved", youthId, "ReflectionEntry", reflection.Id,
            isPrivate: reflection.Visibility == Visibility.Private);

        return new ReflectionResult(reflection.Id);
    }
}

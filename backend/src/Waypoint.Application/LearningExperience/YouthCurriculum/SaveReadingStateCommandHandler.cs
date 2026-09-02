using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.LearningExperience.YouthCurriculum;

public class SaveReadingStateCommandHandler : IRequestHandler<SaveReadingStateCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public SaveReadingStateCommandHandler(IAppDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task Handle(SaveReadingStateCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();

        await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
            _db,
            request.LessonId,
            youthId,
            cancellationToken);

        var now = _clock.UtcNow;

        var existing = await _db.LessonReadingStates
            .FirstOrDefaultAsync(r => r.YouthUserId == youthId && r.LessonId == request.LessonId, cancellationToken);

        if (existing is null)
        {
            _db.LessonReadingStates.Add(new LessonReadingState
            {
                YouthUserId = youthId,
                LessonId = request.LessonId,
                TextSize = request.TextSize,
                Bookmarked = request.Bookmarked,
                Position = request.Position,
                UpdatedAt = now,
            });
        }
        else
        {
            existing.TextSize = request.TextSize;
            existing.Bookmarked = request.Bookmarked;
            existing.Position = request.Position;
            existing.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}

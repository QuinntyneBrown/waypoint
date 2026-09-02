using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;
using Waypoint.Application.LearningExperience.YouthCurriculum;

namespace Waypoint.Application.LearningExperience.Reflections;

public class UpdateReflectionCommandHandler : IRequestHandler<UpdateReflectionCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public UpdateReflectionCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
    }

    public async Task Handle(UpdateReflectionCommand request, CancellationToken cancellationToken)
    {
        var reflection = await _db.Reflections.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Reflection not found.");

        if (reflection.YouthUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This reflection does not belong to you.");
        }

        await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
            _db, reflection.LessonId, reflection.YouthUserId, cancellationToken);

        reflection.Text = request.Text;
        reflection.Mood = request.Mood;
        reflection.SecondaryJson = ReflectionSecondaryResponseSerializer.Serialize(request.SecondaryResponses);
        reflection.Visibility = request.ShareWithMentor ? Visibility.SharedWithMentor : Visibility.Private;
        reflection.IsDraft = request.IsDraft;
        reflection.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ReflectionUpdated", _currentUser.UserId, "ReflectionEntry", reflection.Id,
            isPrivate: reflection.Visibility == Visibility.Private);
    }
}

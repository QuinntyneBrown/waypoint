using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.Reflections;

public sealed class SharedReflectionsQueryHandler
    : IRequestHandler<SharedReflectionsQuery, IReadOnlyList<SharedReflectionView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SharedReflectionsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SharedReflectionView>> Handle(
        SharedReflectionsQuery request,
        CancellationToken cancellationToken)
    {
        var youth = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.YouthId && item.Role == Roles.Youth, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var rows = await (
            from reflection in _db.Reflections.AsNoTracking()
            join lesson in _db.Lessons.AsNoTracking() on reflection.LessonId equals lesson.Id
            where reflection.YouthUserId == request.YouthId
                && reflection.Visibility == Visibility.SharedWithMentor
                && !reflection.IsDraft
            orderby reflection.UpdatedAt descending
            select new
            {
                reflection.Id,
                reflection.YouthUserId,
                reflection.LessonId,
                LessonTitle = lesson.Title,
                reflection.Text,
                reflection.Mood,
                reflection.SecondaryJson,
                reflection.UpdatedAt,
            }).ToListAsync(cancellationToken);

        return rows.Select(item => new SharedReflectionView(
            item.Id,
            item.YouthUserId,
            item.LessonId,
            item.LessonTitle,
            item.Text ?? string.Empty,
            item.Mood ?? string.Empty,
            ReflectionSecondaryResponseSerializer.Deserialize(item.SecondaryJson),
            item.UpdatedAt)).ToList();
    }
}

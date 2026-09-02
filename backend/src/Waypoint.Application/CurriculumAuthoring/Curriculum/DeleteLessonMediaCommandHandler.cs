using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed class DeleteLessonMediaCommandHandler : IRequestHandler<DeleteLessonMediaCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public DeleteLessonMediaCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteLessonMediaCommand request, CancellationToken cancellationToken)
    {
        var media = await _db.LessonMedia.FirstOrDefaultAsync(
            item => item.Id == request.MediaId && item.LessonId == request.LessonId,
            cancellationToken) ?? throw new NotFoundException("Lesson media not found.");
        _db.LessonMedia.Remove(media);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "LessonMediaRemoved",
            _currentUser.UserId,
            "LessonMedia",
            media.Id,
            cancellationToken: cancellationToken);
    }
}

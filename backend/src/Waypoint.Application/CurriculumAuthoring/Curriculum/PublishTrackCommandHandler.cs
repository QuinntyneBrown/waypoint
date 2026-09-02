using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed class PublishTrackCommandHandler : IRequestHandler<PublishTrackCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public PublishTrackCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(PublishTrackCommand request, CancellationToken cancellationToken)
    {
        var track = await _db.Tracks.FirstOrDefaultAsync(
            item => item.Id == request.TrackId,
            cancellationToken) ?? throw new NotFoundException("Track not found.");

        track.Status = request.Status;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "TrackPublishStateChanged",
            _currentUser.UserId,
            "Track",
            track.Id,
            details: request.Status.ToString(),
            cancellationToken: cancellationToken);
    }
}

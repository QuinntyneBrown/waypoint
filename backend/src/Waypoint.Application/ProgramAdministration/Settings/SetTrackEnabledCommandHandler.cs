using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ProgramAdministration.Settings;

public class SetTrackEnabledCommandHandler : IRequestHandler<SetTrackEnabledCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public SetTrackEnabledCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(SetTrackEnabledCommand request, CancellationToken cancellationToken)
    {
        var track = await _db.Tracks.FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
            ?? throw new NotFoundException("Track not found.");

        track.Enabled = request.Enabled;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            request.Enabled ? "TrackEnabled" : "TrackDisabled",
            _currentUser.UserId,
            "Track",
            track.Id,
            cancellationToken: cancellationToken);
    }
}

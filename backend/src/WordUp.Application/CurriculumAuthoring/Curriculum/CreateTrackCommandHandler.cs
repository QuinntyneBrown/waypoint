using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public class CreateTrackCommandHandler : IRequestHandler<CreateTrackCommand, TrackResult>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CreateTrackCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<TrackResult> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim();

        // A track key is unique across the catalogue (L2-CURRIC-001 AC1).
        var exists = await _db.Tracks.AnyAsync(t => t.Key == key, cancellationToken);
        if (exists)
        {
            throw new ConflictException($"A track with key '{key}' already exists.");
        }

        var track = new Track
        {
            Name = request.Name.Trim(),
            Key = key,
            Description = request.Description,
            Status = PublishStatus.Draft,
            Enabled = true,
        };

        _db.Tracks.Add(track);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("TrackCreated", _currentUser.UserId, "Track", track.Id);

        return new TrackResult(track.Id);
    }
}

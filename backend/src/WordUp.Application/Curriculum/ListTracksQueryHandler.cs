using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;

namespace WordUp.Application.Curriculum;

public class ListTracksQueryHandler : IRequestHandler<ListTracksQuery, IReadOnlyList<TrackSummary>>
{
    private readonly IAppDbContext _db;

    public ListTracksQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<TrackSummary>> Handle(ListTracksQuery request, CancellationToken cancellationToken)
    {
        return await _db.Tracks
            .OrderBy(t => t.Name)
            .Select(t => new TrackSummary(t.Id, t.Name, t.Key, t.Status.ToString(), t.Enabled))
            .ToListAsync(cancellationToken);
    }
}

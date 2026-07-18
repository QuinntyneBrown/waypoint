using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

public class BrowseCurriculumQueryHandler : IRequestHandler<BrowseCurriculumQuery, CurriculumView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public BrowseCurriculumQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CurriculumView> Handle(BrowseCurriculumQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();

        var enrolledIds = await CurriculumProgressCalculator.ResolveYouthTrackIdsAsync(
            _db,
            youthId,
            cancellationToken);
        var tracks = await _db.Tracks
            .Where(track => enrolledIds.Contains(track.Id) &&
                            track.Enabled &&
                            track.Status == PublishStatus.Published)
            .OrderBy(track => track.Name)
            .ToListAsync(cancellationToken);
        if (tracks.Count == 0)
        {
            return new CurriculumView(Guid.Empty, Array.Empty<TrackOption>(), Array.Empty<ModuleStatus>());
        }

        var primaryTrackId = await CurriculumProgressCalculator.ResolveYouthTrackIdAsync(
            _db,
            youthId,
            cancellationToken);
        var selectedTrackId = request.TrackId ??
            (primaryTrackId is { } primary && tracks.Any(track => track.Id == primary)
                ? primary
                : tracks[0].Id);
        if (tracks.All(track => track.Id != selectedTrackId))
        {
            throw new ForbiddenException("You are not enrolled in this track.");
        }

        var modules = await CurriculumProgressCalculator.BuildModuleStatusesAsync(
            _db, selectedTrackId, youthId, cancellationToken);

        return new CurriculumView(
            selectedTrackId,
            tracks.Select(track => new TrackOption(
                track.Id,
                track.Name,
                track.Description,
                track.Id == selectedTrackId)).ToList(),
            modules);
    }
}

using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public record ListTracksQuery : IRequest<IReadOnlyList<TrackSummary>>;

public record TrackSummary(Guid Id, string Name, string Key, string Status, bool Enabled);

using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record PublishTrackCommand(Guid TrackId, PublishStatus Status) : IRequest;

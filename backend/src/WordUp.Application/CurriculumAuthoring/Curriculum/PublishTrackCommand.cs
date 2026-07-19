using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public sealed record PublishTrackCommand(Guid TrackId, PublishStatus Status) : IRequest;

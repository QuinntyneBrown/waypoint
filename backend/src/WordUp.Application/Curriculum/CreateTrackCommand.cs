using MediatR;

namespace WordUp.Application.Curriculum;

public record CreateTrackCommand(string Name, string Key, string? Description) : IRequest<TrackResult>;

public record TrackResult(Guid Id);

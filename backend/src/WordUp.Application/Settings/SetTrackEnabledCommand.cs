using MediatR;

namespace WordUp.Application.Settings;

public record SetTrackEnabledCommand(Guid TrackId, bool Enabled) : IRequest;

using MediatR;

namespace Waypoint.Application.ProgramAdministration.Settings;

public record SetTrackEnabledCommand(Guid TrackId, bool Enabled) : IRequest;

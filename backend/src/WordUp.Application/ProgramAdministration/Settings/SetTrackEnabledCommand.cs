using MediatR;

namespace WordUp.Application.ProgramAdministration.Settings;

public record SetTrackEnabledCommand(Guid TrackId, bool Enabled) : IRequest;

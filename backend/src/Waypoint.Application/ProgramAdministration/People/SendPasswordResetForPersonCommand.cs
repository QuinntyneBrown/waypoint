using MediatR;

namespace Waypoint.Application.ProgramAdministration.People;

public record SendPasswordResetForPersonCommand(Guid Id) : IRequest, IAtomicOutboxRequest;

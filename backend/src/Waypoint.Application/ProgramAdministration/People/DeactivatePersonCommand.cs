using MediatR;

namespace Waypoint.Application.ProgramAdministration.People;

public record DeactivatePersonCommand(Guid Id) : IRequest;

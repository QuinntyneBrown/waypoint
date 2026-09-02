using MediatR;

namespace Waypoint.Application.ProgramAdministration.People;

public record RemovePersonCommand(Guid Id) : IRequest;

using MediatR;

namespace Waypoint.Application.ProgramAdministration.People;

public record ReactivatePersonCommand(Guid Id) : IRequest;

using MediatR;

namespace Waypoint.Application.ProgramAdministration.People;

public record ChangeRoleCommand(Guid Id, string Role) : IRequest;

using MediatR;

namespace Waypoint.Application.ProgramAdministration.People;

public record LinkGuardianCommand(Guid YouthId, Guid GuardianId) : IRequest;

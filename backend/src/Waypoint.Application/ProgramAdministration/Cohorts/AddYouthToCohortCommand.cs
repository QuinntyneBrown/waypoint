using MediatR;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

public record AddYouthToCohortCommand(Guid CohortId, Guid YouthId) : IRequest;

using MediatR;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public record AddYouthToCohortCommand(Guid CohortId, Guid YouthId) : IRequest;

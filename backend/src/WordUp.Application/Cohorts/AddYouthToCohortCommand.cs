using MediatR;

namespace WordUp.Application.Cohorts;

public record AddYouthToCohortCommand(Guid CohortId, Guid YouthId) : IRequest;

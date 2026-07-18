using MediatR;
using WordUp.Domain;

namespace WordUp.Application.CompetencyAssessing;

public record SetCompetencyLevelCommand(
    Guid YouthId,
    Guid CompetencyId,
    CompetencyLevel Level,
    string? Notes) : IRequest;

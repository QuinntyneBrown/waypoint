using MediatR;

namespace WordUp.Application.Mentoring.Competencies;

public record SetCompetencyLevelCommand(
    Guid YouthId,
    Guid CompetencyId,
    CompetencyLevel Level,
    string? Notes) : IRequest;

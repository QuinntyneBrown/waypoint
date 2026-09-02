using MediatR;

namespace Waypoint.Application.Mentoring.Competencies;

public record SetCompetencyLevelCommand(
    Guid YouthId,
    Guid CompetencyId,
    CompetencyLevel Level,
    string? Notes) : IRequest;

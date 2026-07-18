using MediatR;

namespace WordUp.Application.CompetencyAssessing;

public record YouthCompetenciesQuery(Guid YouthId) : IRequest<CompetencySummary>;

public record CompetencySummary(IReadOnlyList<CompetencyLevelDto> Items, int MasteredOrProficient);

public record CompetencyLevelDto(Guid CompetencyId, string Level, string? Notes);

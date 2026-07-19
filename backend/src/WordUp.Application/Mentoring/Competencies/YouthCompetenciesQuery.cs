using MediatR;

namespace WordUp.Application.Mentoring.Competencies;

public record YouthCompetenciesQuery(Guid YouthId) : IRequest<CompetencySummary>;

public record CompetencySummary(
    Guid YouthId,
    string YouthName,
    string? CurrentStageName,
    int CurrentStageProgress,
    IReadOnlyList<CompetencyLevelDto> Items,
    int MasteredOrProficient,
    int Total);

public record CompetencyLevelDto(
    Guid CompetencyId,
    string Name,
    string StageName,
    string Level,
    string? Notes,
    string? AssessedByName,
    DateTime? AssessedAt);

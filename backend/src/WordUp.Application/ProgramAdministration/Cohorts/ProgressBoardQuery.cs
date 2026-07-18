using MediatR;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public record ProgressBoardQuery(Guid CohortId) : IRequest<ProgressBoardResult>;

public record ProgressBoardResult(
    IReadOnlyList<ModuleColumn> Modules,
    IReadOnlyList<YouthRow> Rows,
    double CohortAveragePercent);

public record ModuleColumn(Guid ModuleId, string Name);

public record YouthRow(
    Guid YouthId,
    string YouthName,
    IReadOnlyList<CellState> Cells,
    double QuizAverage,
    double OverallPercent);

public record CellState(Guid ModuleId, string State);

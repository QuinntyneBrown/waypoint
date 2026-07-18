using MediatR;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public record ProgressBoardQuery(Guid CohortId, Guid? TrackId = null) : IRequest<ProgressBoardResult>;

public record ProgressBoardResult(
    Guid CohortId,
    string CohortName,
    Guid TrackId,
    string TrackName,
    IReadOnlyList<ModuleColumn> Modules,
    IReadOnlyList<YouthRow> Rows,
    double CohortAveragePercent);

public record ModuleColumn(Guid ModuleId, string Name, double AveragePercent);

public record YouthRow(
    Guid YouthId,
    string YouthName,
    IReadOnlyList<CellState> Cells,
    double QuizAverage,
    double OverallPercent);

public record CellState(Guid ModuleId, string State, double Percent);

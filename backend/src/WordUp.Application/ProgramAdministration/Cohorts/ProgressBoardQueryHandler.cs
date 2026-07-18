using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.Cohorts;

/// <summary>
/// Builds the cohort progress board (L2-ADMIN-004 AC1). Columns are the cohort track's published
/// modules; rows are the cohort's youth. Youth progress data does not exist yet (arrives in LEARN/M3),
/// so every cell is "NotStarted" and all aggregate percentages are 0.
/// </summary>
public class ProgressBoardQueryHandler : IRequestHandler<ProgressBoardQuery, ProgressBoardResult>
{
    private const string NotStarted = "NotStarted";
    private readonly IAppDbContext _db;

    public ProgressBoardQueryHandler(IAppDbContext db) => _db = db;

    public async Task<ProgressBoardResult> Handle(ProgressBoardQuery request, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.Id == request.CohortId, cancellationToken)
            ?? throw new NotFoundException("Cohort not found.");

        var modules = await _db.Modules
            .Where(m => m.TrackId == cohort.TrackId && m.Status == PublishStatus.Published)
            .OrderBy(m => m.Order)
            .Select(m => new ModuleColumn(m.Id, m.Name))
            .ToListAsync(cancellationToken);

        var youth = await _db.Users
            .Where(u => u.CohortId == cohort.Id && u.Role == Roles.Youth)
            .OrderBy(u => u.DisplayName)
            .Select(u => new { u.Id, u.DisplayName })
            .ToListAsync(cancellationToken);

        var rows = youth
            .Select(u => new YouthRow(
                u.Id,
                u.DisplayName,
                modules.Select(m => new CellState(m.ModuleId, NotStarted)).ToList(),
                QuizAverage: 0,
                OverallPercent: 0))
            .ToList();

        return new ProgressBoardResult(modules, rows, CohortAveragePercent: 0);
    }
}

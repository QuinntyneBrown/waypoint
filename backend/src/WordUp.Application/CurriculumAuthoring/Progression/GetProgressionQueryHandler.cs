using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public sealed class GetProgressionQueryHandler
    : IRequestHandler<GetProgressionQuery, IReadOnlyList<ProgressionStageView>>
{
    private readonly IAppDbContext _db;

    public GetProgressionQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProgressionStageView>> Handle(
        GetProgressionQuery request,
        CancellationToken cancellationToken)
    {
        var stages = await _db.ProgressionStages
            .AsNoTracking()
            .OrderBy(item => item.Order)
            .ThenBy(item => item.Name)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var stageIds = stages.Select(item => item.Id).ToList();
        var competencies = await _db.Competencies
            .AsNoTracking()
            .Where(item => stageIds.Contains(item.StageId) && item.ArchivedAt == null)
            .OrderBy(item => item.Order)
            .ThenBy(item => item.Name)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

        return stages.Select(stage => new ProgressionStageView(
                stage.Id,
                stage.Name,
                stage.Description,
                stage.Order,
                competencies
                    .Where(item => item.StageId == stage.Id)
                    .Select(item => new ProgressionCompetencyView(item.Id, item.Name, item.Order))
                    .ToArray()))
            .ToArray();
    }
}

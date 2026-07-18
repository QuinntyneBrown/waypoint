using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Progression;

public class AddCompetencyCommandHandler : IRequestHandler<AddCompetencyCommand, CompetencyResult>
{
    private readonly IAppDbContext _db;

    public AddCompetencyCommandHandler(IAppDbContext db) => _db = db;

    public async Task<CompetencyResult> Handle(AddCompetencyCommand request, CancellationToken cancellationToken)
    {
        var stageExists = await _db.ProgressionStages.AnyAsync(s => s.Id == request.StageId, cancellationToken);
        if (!stageExists)
        {
            throw new NotFoundException("Progression stage not found.");
        }

        var nextOrder = await _db.Competencies.CountAsync(c => c.StageId == request.StageId, cancellationToken);

        var competency = new Competency
        {
            StageId = request.StageId,
            Name = request.Name.Trim(),
            Order = nextOrder,
        };

        _db.Competencies.Add(competency);
        await _db.SaveChangesAsync(cancellationToken);

        return new CompetencyResult(competency.Id);
    }
}

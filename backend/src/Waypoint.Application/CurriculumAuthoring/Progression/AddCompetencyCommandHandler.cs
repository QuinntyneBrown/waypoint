using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Progression;

public class AddCompetencyCommandHandler : IRequestHandler<AddCompetencyCommand, CompetencyResult>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public AddCompetencyCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<CompetencyResult> Handle(AddCompetencyCommand request, CancellationToken cancellationToken)
    {
        var stageExists = await _db.ProgressionStages.AnyAsync(s => s.Id == request.StageId, cancellationToken);
        if (!stageExists)
        {
            throw new NotFoundException("Progression stage not found.");
        }

        var nextOrder = await _db.Competencies.CountAsync(
            item => item.StageId == request.StageId && item.ArchivedAt == null,
            cancellationToken);

        var competency = new Competency
        {
            StageId = request.StageId,
            Name = request.Name.Trim(),
            Order = nextOrder,
        };

        _db.Competencies.Add(competency);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "CompetencyAdded",
            _currentUser.UserId,
            "Competency",
            competency.Id,
            cancellationToken: cancellationToken);

        return new CompetencyResult(competency.Id);
    }
}

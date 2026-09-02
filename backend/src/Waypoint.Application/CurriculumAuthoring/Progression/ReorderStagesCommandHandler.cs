using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Progression;

public class ReorderStagesCommandHandler : IRequestHandler<ReorderStagesCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ReorderStagesCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ReorderStagesCommand request, CancellationToken cancellationToken)
    {
        var ids = request.OrderedStageIds ?? Array.Empty<Guid>();
        var stages = await _db.ProgressionStages.ToListAsync(cancellationToken);
        var savedIds = stages.Select(item => item.Id).ToHashSet();
        if (ids.Count != stages.Count
            || ids.Distinct().Count() != ids.Count
            || ids.Any(id => !savedIds.Contains(id)))
        {
            throw new ConflictException("Stage reorder must contain every saved stage exactly once.");
        }

        for (var i = 0; i < ids.Count; i++)
        {
            stages.Single(item => item.Id == ids[i]).Order = i + 1;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ProgressionStagesReordered",
            _currentUser.UserId,
            "ProgressionStage",
            null,
            cancellationToken: cancellationToken);
    }
}

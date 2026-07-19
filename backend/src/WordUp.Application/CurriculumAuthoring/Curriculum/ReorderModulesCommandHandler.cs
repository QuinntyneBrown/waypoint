using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public class ReorderModulesCommandHandler : IRequestHandler<ReorderModulesCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ReorderModulesCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ReorderModulesCommand request, CancellationToken cancellationToken)
    {
        var modules = await _db.Modules
            .Where(m => m.TrackId == request.TrackId)
            .ToListAsync(cancellationToken);

        var requestedIds = request.OrderedModuleIds?.ToList() ?? [];
        if (requestedIds.Count != modules.Count
            || requestedIds.Distinct().Count() != requestedIds.Count
            || requestedIds.Except(modules.Select(item => item.Id)).Any())
        {
            throw new ConflictException("The module order must contain every module exactly once.");
        }
        var proposedOrder = requestedIds
            .Select((id, index) => new { id, Order = index + 1 })
            .ToDictionary(item => item.id, item => item.Order);
        if (modules.Any(module => module.PrerequisiteModuleId is { } prerequisiteId
                && (!proposedOrder.TryGetValue(prerequisiteId, out var prerequisiteOrder)
                    || prerequisiteOrder >= proposedOrder[module.Id])))
        {
            throw new ConflictException("A prerequisite must remain before its dependent module.");
        }

        // Persist the caller's sequence as the new order (L2-CURRIC-001 AC3).
        for (var i = 0; i < requestedIds.Count; i++)
        {
            var module = modules.First(m => m.Id == requestedIds[i]);
            module.Order = i + 1;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "ModulesReordered",
            _currentUser.UserId,
            "Track",
            request.TrackId,
            cancellationToken: cancellationToken);
    }
}

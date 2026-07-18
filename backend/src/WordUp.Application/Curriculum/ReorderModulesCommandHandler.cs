using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.Curriculum;

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

        // Persist the caller's sequence as the new order (L2-CURRIC-001 AC3).
        for (var i = 0; i < request.OrderedModuleIds.Count; i++)
        {
            var module = modules.FirstOrDefault(m => m.Id == request.OrderedModuleIds[i]);
            if (module is null)
            {
                throw new NotFoundException($"Module {request.OrderedModuleIds[i]} is not part of this track.");
            }

            module.Order = i + 1;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("ModulesReordered", _currentUser.UserId, "Track", request.TrackId);
    }
}

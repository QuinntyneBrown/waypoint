using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.Curriculum;

public class PublishModuleCommandHandler : IRequestHandler<PublishModuleCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public PublishModuleCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(PublishModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _db.Modules.FirstOrDefaultAsync(m => m.Id == request.ModuleId, cancellationToken);
        if (module is null)
        {
            throw new NotFoundException("Module not found.");
        }

        // Move the module through its publish lifecycle (L2-CURRIC-001 AC4).
        module.Status = request.Status;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "ModulePublishStateChanged", _currentUser.UserId, "Module", module.Id, details: request.Status.ToString());
    }
}

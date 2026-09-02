using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public UpdateModuleCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _db.Modules.FirstOrDefaultAsync(
            item => item.Id == request.ModuleId,
            cancellationToken) ?? throw new NotFoundException("Module not found.");
        if (module.TrackId != request.TrackId)
        {
            throw new ConflictException("A module cannot be moved to another track while editing it.");
        }

        await EnsureValidPrerequisiteAsync(module, request.PrerequisiteModuleId, cancellationToken);
        module.Name = request.Name.Trim();
        module.Description = Clean(request.Description);
        module.PrerequisiteModuleId = request.PrerequisiteModuleId;
        module.ScriptureAnchor = Clean(request.ScriptureAnchor);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ModuleUpdated",
            _currentUser.UserId,
            "Module",
            module.Id,
            cancellationToken: cancellationToken);
    }

    private async Task EnsureValidPrerequisiteAsync(
        Module module,
        Guid? prerequisiteModuleId,
        CancellationToken cancellationToken)
    {
        if (prerequisiteModuleId is not { } prerequisiteId) return;
        var prerequisite = await _db.Modules.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == prerequisiteId,
            cancellationToken);
        if (prerequisite is null
            || prerequisite.TrackId != module.TrackId
            || prerequisite.Id == module.Id
            || prerequisite.Order >= module.Order)
        {
            throw new ConflictException("The prerequisite must be an earlier module in this track.");
        }
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

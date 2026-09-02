using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, ModuleResult>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CreateModuleCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<ModuleResult> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var track = await _db.Tracks.FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken);
        if (track is null)
        {
            throw new NotFoundException("Track not found.");
        }

        if (request.PrerequisiteModuleId is { } prerequisiteId
            && !await _db.Modules.AnyAsync(
                item => item.Id == prerequisiteId && item.TrackId == request.TrackId,
                cancellationToken))
        {
            throw new ConflictException("The prerequisite must be an earlier module in this track.");
        }

        // New modules append to the end of the track's ordered list (L2-CURRIC-001 AC2).
        var maxOrder = await _db.Modules
            .Where(m => m.TrackId == request.TrackId)
            .Select(m => (int?)m.Order)
            .MaxAsync(cancellationToken) ?? 0;

        var module = new Module
        {
            TrackId = request.TrackId,
            Name = request.Name.Trim(),
            Description = Clean(request.Description),
            PrerequisiteModuleId = request.PrerequisiteModuleId,
            ScriptureAnchor = Clean(request.ScriptureAnchor),
            Order = maxOrder + 1,
            Status = PublishStatus.Draft,
        };

        _db.Modules.Add(module);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "ModuleCreated",
            _currentUser.UserId,
            "Module",
            module.Id,
            cancellationToken: cancellationToken);

        return new ModuleResult(module.Id);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

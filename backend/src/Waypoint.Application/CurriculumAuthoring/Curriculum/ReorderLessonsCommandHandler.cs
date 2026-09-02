using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed class ReorderLessonsCommandHandler : IRequestHandler<ReorderLessonsCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ReorderLessonsCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ReorderLessonsCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Modules.AnyAsync(item => item.Id == request.ModuleId, cancellationToken))
        {
            throw new NotFoundException("Module not found.");
        }

        var lessons = await _db.Lessons
            .Where(item => item.ModuleId == request.ModuleId && item.ArchivedAt == null)
            .ToListAsync(cancellationToken);
        var requestedIds = request.OrderedLessonIds?.ToList() ?? [];
        if (requestedIds.Count != lessons.Count
            || requestedIds.Distinct().Count() != requestedIds.Count
            || requestedIds.Except(lessons.Select(item => item.Id)).Any())
        {
            throw new ConflictException("The lesson order must contain every active lesson exactly once.");
        }

        var byId = lessons.ToDictionary(item => item.Id);
        for (var index = 0; index < requestedIds.Count; index++)
        {
            byId[requestedIds[index]].Order = index + 1;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "LessonsReordered",
            _currentUser.UserId,
            "Module",
            request.ModuleId,
            cancellationToken: cancellationToken);
    }
}

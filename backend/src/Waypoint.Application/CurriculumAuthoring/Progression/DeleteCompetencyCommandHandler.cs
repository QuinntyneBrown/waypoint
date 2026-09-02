using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Progression;

public sealed class DeleteCompetencyCommandHandler : IRequestHandler<DeleteCompetencyCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public DeleteCompetencyCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteCompetencyCommand request, CancellationToken cancellationToken)
    {
        var competency = await _db.Competencies.FirstOrDefaultAsync(
            item => item.Id == request.CompetencyId && item.ArchivedAt == null,
            cancellationToken)
            ?? throw new NotFoundException("Competency not found.");

        competency.ArchivedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "CompetencyArchived",
            _currentUser.UserId,
            "Competency",
            competency.Id,
            cancellationToken: cancellationToken);
    }
}

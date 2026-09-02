using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ProgramAdministration.People;

public class ReactivatePersonCommandHandler : IRequestHandler<ReactivatePersonCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ReactivatePersonCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ReactivatePersonCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");
        if (user.Status == UserStatus.Removed)
        {
            throw new ConflictException("Removal is irreversible; this person cannot be reactivated.");
        }
        if (user.Status == UserStatus.Active)
        {
            return;
        }

        user.Status = UserStatus.Active;
        user.DeactivatedAt = null;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("AccountReactivated", _currentUser.UserId, "User", user.Id);
    }
}

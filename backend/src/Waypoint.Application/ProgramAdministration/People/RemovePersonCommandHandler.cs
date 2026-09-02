using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ProgramAdministration.People;

/// <summary>
/// Removes a person from the active program. Records are retained (soft removal via deactivation)
/// so historical data is preserved (L2-ADMIN-002 AC3).
/// </summary>
public class RemovePersonCommandHandler : IRequestHandler<RemovePersonCommand>
{
    private readonly IAppDbContext _db;
    private readonly ISessionIssuer _sessions;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public RemovePersonCommandHandler(
        IAppDbContext db,
        ISessionIssuer sessions,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IClock clock)
    {
        _db = db;
        _sessions = sessions;
        _audit = audit;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(RemovePersonCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");
        if (_currentUser.UserId == user.Id)
        {
            throw new ForbiddenException("You cannot remove your own account.");
        }
        if (user.Status == UserStatus.Removed)
        {
            throw new ConflictException("This person has already been removed.");
        }

        user.Status = UserStatus.Removed;
        user.DeactivatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _sessions.RevokeAllAsync(user.Id, cancellationToken);
        await _audit.LogAsync("AccountRemoved", _currentUser.UserId, "User", user.Id);
    }
}

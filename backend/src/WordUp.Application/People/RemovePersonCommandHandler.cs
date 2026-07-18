using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.People;

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

    public RemovePersonCommandHandler(
        IAppDbContext db,
        ISessionIssuer sessions,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _sessions = sessions;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(RemovePersonCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");

        user.Status = UserStatus.Deactivated;
        await _db.SaveChangesAsync(cancellationToken);

        await _sessions.RevokeAllAsync(user.Id, cancellationToken);
        await _audit.LogAsync("AccountRemoved", _currentUser.UserId, "User", user.Id);
    }
}

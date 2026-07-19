using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.People;

public class DeactivatePersonCommandHandler : IRequestHandler<DeactivatePersonCommand>
{
    private readonly IAppDbContext _db;
    private readonly ISessionIssuer _sessions;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public DeactivatePersonCommandHandler(
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

    public async Task Handle(DeactivatePersonCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");
        if (user.Status == UserStatus.Removed)
        {
            throw new ConflictException("A removed person cannot be deactivated.");
        }
        if (user.Status == UserStatus.Deactivated)
        {
            return;
        }

        user.Status = UserStatus.Deactivated;
        user.DeactivatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _sessions.RevokeAllAsync(user.Id, cancellationToken);
        await _audit.LogAsync("AccountDeactivated", _currentUser.UserId, "User", user.Id);
    }
}

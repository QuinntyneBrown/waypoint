using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Application.Common;

namespace WordUp.Application.Profile;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _hasher;
    private readonly ISessionIssuer _session;
    private readonly IAuditLogger _audit;

    public ChangePasswordCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IPasswordHasher hasher,
        ISessionIssuer session,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _hasher = hasher;
        _session = session;
        _audit = audit;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // The current password must match before we allow a change.
        if (!_hasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new ForbiddenException("The current password is incorrect.");
        }

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        await _db.SaveChangesAsync(cancellationToken);

        // Invalidate every other active session for this user.
        await _session.RevokeAllAsync(user.Id, cancellationToken);
        await _audit.LogAsync("PasswordChanged", userId, "User", userId, cancellationToken: cancellationToken);
    }
}

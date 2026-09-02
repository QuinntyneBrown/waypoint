using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.Authentication.Auth;

public class SignInCommandHandler : IRequestHandler<SignInCommand, AuthResult>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;
    private readonly ISessionIssuer _session;
    private readonly IAuditLogger _audit;

    public SignInCommandHandler(
        IAppDbContext db,
        IPasswordHasher hasher,
        IClock clock,
        ISessionIssuer session,
        IAuditLogger audit)
    {
        _db = db;
        _hasher = hasher;
        _clock = clock;
        _session = session;
        _audit = audit;
    }

    public async Task<AuthResult> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var now = _clock.UtcNow;

        // AC2: unrecognized email -> generic failure (no enumeration).
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (user.Status != UserStatus.Active)
        {
            throw new AccountDeactivatedException();
        }

        if (user.IsLockedOut(now))
        {
            throw new AccountLockedException(LockoutPolicy.LockMinutes);
        }

        if (!_hasher.Verify(request.Password, user.PasswordHash))
        {
            // AC3: increment failed-attempt counter; lock at the threshold.
            user.FailedSignInAttempts++;
            if (user.FailedSignInAttempts >= LockoutPolicy.MaxFailedAttempts)
            {
                user.LockoutEndsAt = now.AddMinutes(LockoutPolicy.LockMinutes);
                user.FailedSignInAttempts = 0;
                await _db.SaveChangesAsync(cancellationToken);
                await _audit.LogAsync("AccountLocked", user.Id, "User", user.Id);
                throw new AccountLockedException(LockoutPolicy.LockMinutes);
            }

            await _db.SaveChangesAsync(cancellationToken);
            throw new InvalidCredentialsException();
        }

        // Success: clear failure state, issue a session, audit.
        user.FailedSignInAttempts = 0;
        user.LockoutEndsAt = null;
        user.LastSignInAt = now;
        await _db.SaveChangesAsync(cancellationToken);

        var result = await _session.IssueAsync(user, cancellationToken);
        await _audit.LogAsync("SignIn", user.Id, "User", user.Id);
        return result;
    }
}

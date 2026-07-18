using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;

namespace WordUp.Application.PasswordReset;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;
    private readonly ISessionIssuer _session;
    private readonly IAuditLogger _audit;

    public ResetPasswordCommandHandler(
        IAppDbContext db,
        ISecureTokens tokens,
        IPasswordHasher hasher,
        IClock clock,
        ISessionIssuer session,
        IAuditLogger audit)
    {
        _db = db;
        _tokens = tokens;
        _hasher = hasher;
        _clock = clock;
        _session = session;
        _audit = audit;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokens.Hash(request.Token);
        var now = _clock.UtcNow;

        var token = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (token is null || !token.IsUsable(now))
        {
            // Expired, already used, or unknown (L2-AUTH-005).
            throw new ResetLinkInvalidException();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == token.UserId, cancellationToken);
        if (user is null)
        {
            throw new ResetLinkInvalidException();
        }

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        token.UsedAt = now;

        // Invalidate any existing sessions/refresh tokens (L2-AUTH-005 AC3).
        await _session.RevokeAllAsync(user.Id, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("PasswordReset", user.Id, "User", user.Id);
    }
}

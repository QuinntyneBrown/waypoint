using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.Authentication.Auth;

public sealed class SessionIssuer : ISessionIssuer
{
    private const int RefreshTokenDays = 30;

    private readonly IAppDbContext _db;
    private readonly IJwtTokenIssuer _jwt;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;

    public SessionIssuer(IAppDbContext db, IJwtTokenIssuer jwt, ISecureTokens tokens, IClock clock)
    {
        _db = db;
        _jwt = jwt;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<AuthResult> IssueAsync(User user, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var (raw, hash) = _tokens.Create();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            FamilyId = Guid.NewGuid(),
            TokenHash = hash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(RefreshTokenDays),
        });
        await _db.SaveChangesAsync(cancellationToken);
        return Build(user, raw);
    }

    public async Task<AuthResult> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var hash = _tokens.Hash(rawRefreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (token is null)
        {
            throw new InvalidRefreshTokenException();
        }

        // Reuse detection: a rotated (replaced) or revoked token being presented => compromise; kill the family.
        if (token.ReplacedByTokenHash is not null || token.RevokedAt is not null)
        {
            await RevokeFamilyAsync(token.FamilyId, now, cancellationToken);
            throw new InvalidRefreshTokenException();
        }

        if (token.ExpiresAt <= now)
        {
            throw new InvalidRefreshTokenException();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == token.UserId, cancellationToken)
            ?? throw new InvalidRefreshTokenException();
        if (user.Status != UserStatus.Active)
        {
            throw new InvalidRefreshTokenException();
        }

        var (raw, newHash) = _tokens.Create();
        token.RevokedAt = now;
        token.ReplacedByTokenHash = newHash;
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            FamilyId = token.FamilyId,
            TokenHash = newHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(RefreshTokenDays),
        });
        await _db.SaveChangesAsync(cancellationToken);
        return Build(user, raw);
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is not null)
        {
            user.SessionVersion++;
        }
        var active = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var t in active)
        {
            t.RevokedAt = now;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken cancellationToken)
    {
        var family = await _db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var t in family)
        {
            t.RevokedAt = now;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    private AuthResult Build(User user, string rawRefreshToken)
    {
        var (access, expiresAt) = _jwt.Issue(user);
        return new AuthResult(access, expiresAt, rawRefreshToken, user.Id, user.Email, user.DisplayName, user.Role);
    }
}

using WordUp.Domain;

namespace WordUp.Application.Auth;

/// <summary>
/// Issues and rotates sessions: a short-lived access token plus a rotating, hashed-at-rest refresh
/// token, with family-based reuse detection (L2-AUTH-012).
/// </summary>
public interface ISessionIssuer
{
    /// <summary>Starts a new session (new refresh-token family) for the user.</summary>
    Task<AuthResult> IssueAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>Validates and rotates a presented refresh token; revokes the family on reuse.</summary>
    Task<AuthResult> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revokes all active refresh tokens for a user (sign-out, password change/reset).</summary>
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
}

namespace WordUp.Application.Shared.Abstractions;

/// <summary>
/// Generates cryptographically-random tokens and their at-rest hashes for invitations, password-reset
/// links, and refresh tokens (L2-AUTH-004 AC5, L2-AUTH-012). The raw token is delivered to the user;
/// only the hash is stored.
/// </summary>
public interface ISecureTokens
{
    /// <summary>A fresh random token to deliver, paired with the hash to persist.</summary>
    (string Token, string Hash) Create();

    /// <summary>Hashes a presented token so it can be matched against a stored hash.</summary>
    string Hash(string token);
}

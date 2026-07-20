namespace WordUp.Application.Shared.Abstractions;

/// <summary>Authenticated encryption for sensitive values persisted at rest.</summary>
public interface ISensitiveDataProtector
{
    /// <summary>
    /// Stable, non-secret identifier for the active protection key. Infrastructure uses this only
    /// to prevent cached persistence metadata from crossing protection-key boundaries.
    /// </summary>
    string KeyIdentifier { get; }

    string Protect(string plaintext);
    string Unprotect(string protectedValue);
    byte[] Protect(byte[] plaintext);
    byte[] Unprotect(byte[] protectedValue);
}

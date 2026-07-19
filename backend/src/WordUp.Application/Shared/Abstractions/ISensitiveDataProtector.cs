namespace WordUp.Application.Shared.Abstractions;

/// <summary>Authenticated encryption for sensitive values persisted at rest.</summary>
public interface ISensitiveDataProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}

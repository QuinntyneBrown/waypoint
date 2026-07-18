namespace WordUp.Application.Auth;

/// <summary>
/// Canonical password policy (L2-AUTH-008): at least 8 characters including at least one letter and
/// one number. Enforced identically at invite-accept, reset, and change.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    public const string Message =
        "Password must be at least 8 characters and include at least one letter and one number.";

    public static bool IsCompliant(string? password) =>
        !string.IsNullOrEmpty(password)
        && password.Length >= MinLength
        && password.Any(char.IsLetter)
        && password.Any(char.IsDigit);
}

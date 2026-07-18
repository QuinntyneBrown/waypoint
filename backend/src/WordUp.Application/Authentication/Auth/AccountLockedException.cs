using WordUp.Application.Shared.Common;

namespace WordUp.Application.Authentication.Auth;

/// <summary>Sign-in attempted against a locked account (L2-AUTH-006). 423 Locked.</summary>
public sealed class AccountLockedException : AppException
{
    public AccountLockedException(int lockMinutes)
        : base(423, $"This account is temporarily locked. Try again in about {lockMinutes} minutes, or reset your password.")
    {
        LockMinutes = lockMinutes;
    }

    public int LockMinutes { get; }
}

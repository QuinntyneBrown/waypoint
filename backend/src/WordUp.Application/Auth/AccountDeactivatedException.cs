using WordUp.Application.Common;

namespace WordUp.Application.Auth;

/// <summary>Sign-in attempted against a deactivated account (L2-ADMIN-002 AC2). 403.</summary>
public sealed class AccountDeactivatedException : AppException
{
    public AccountDeactivatedException()
        : base(403, "This account is inactive. Please contact your program director.") { }
}

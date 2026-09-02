using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Authentication.Auth;

/// <summary>Sign-in attempted against a deactivated account (L2-ADMIN-002 AC2). 403.</summary>
public sealed class AccountDeactivatedException : AppException
{
    public AccountDeactivatedException()
        : base(403, "This account is inactive. Please contact your program director.") { }
}

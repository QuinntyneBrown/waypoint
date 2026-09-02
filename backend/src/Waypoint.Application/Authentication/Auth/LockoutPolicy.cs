namespace Waypoint.Application.Authentication.Auth;

/// <summary>Account lockout thresholds (L2-AUTH-006).</summary>
public static class LockoutPolicy
{
    public const int MaxFailedAttempts = 5;
    public const int LockMinutes = 15;
}

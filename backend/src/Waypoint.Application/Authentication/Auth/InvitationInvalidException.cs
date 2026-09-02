using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Authentication.Auth;

/// <summary>An invitation link is expired, revoked, already used, or unknown (L2-AUTH-004). 410 Gone.</summary>
public sealed class InvitationInvalidException : AppException
{
    public InvitationInvalidException(string message = "This invitation is no longer valid. Please request a new invite.")
        : base(410, message) { }
}

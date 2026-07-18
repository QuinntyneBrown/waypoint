using WordUp.Application.Shared.Common;

namespace WordUp.Application.Authentication.PasswordReset;

/// <summary>A password-reset link is expired, already used, or unknown (L2-AUTH-005). 410 Gone.</summary>
public sealed class ResetLinkInvalidException : AppException
{
    public ResetLinkInvalidException()
        : base(410, "This reset link is invalid or has expired.") { }
}

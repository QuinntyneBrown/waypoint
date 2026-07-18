using WordUp.Application.Shared.Common;

namespace WordUp.Application.Authentication.Auth;

/// <summary>Refresh failed: token unknown, expired, or reused after rotation (L2-AUTH-012). 401.</summary>
public sealed class InvalidRefreshTokenException : AppException
{
    public InvalidRefreshTokenException()
        : base(401, "Your session is no longer valid. Please sign in again.") { }
}

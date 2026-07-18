using WordUp.Application.Common;

namespace WordUp.Application.Auth;

/// <summary>Refresh failed: token unknown, expired, or reused after rotation (L2-AUTH-012). 401.</summary>
public sealed class InvalidRefreshTokenException : AppException
{
    public InvalidRefreshTokenException()
        : base(401, "Your session is no longer valid. Please sign in again.") { }
}

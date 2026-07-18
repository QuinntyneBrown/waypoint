namespace WordUp.Application.Auth;

public record AuthResult(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email,
    string DisplayName,
    string Role);

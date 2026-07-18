
namespace WordUp.Application.Shared.Abstractions;

public interface IJwtTokenIssuer
{
    (string AccessToken, DateTime ExpiresAt) Issue(User user);
}

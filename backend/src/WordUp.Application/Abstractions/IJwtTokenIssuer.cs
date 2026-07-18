using WordUp.Domain;

namespace WordUp.Application.Abstractions;

public interface IJwtTokenIssuer
{
    (string AccessToken, DateTime ExpiresAt) Issue(User user);
}

using System.Security.Cryptography;
using System.Text;
using WordUp.Application.Abstractions;

namespace WordUp.Infrastructure;

public sealed class SecureTokens : ISecureTokens
{
    public (string Token, string Hash) Create()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Base64UrlEncode(bytes);
        return (token, Hash(token));
    }

    public string Hash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

public sealed class AesSensitiveDataProtector : ISensitiveDataProtector
{
    private const string Prefix = "enc:v1:";
    private static readonly byte[] AssociatedData = Encoding.UTF8.GetBytes("WordUp.SensitiveData.v1");
    private readonly byte[] _key;

    public AesSensitiveDataProtector(IOptions<SensitiveDataOptions> options)
    {
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(options.Value.EncryptionKey));
    }

    public string Protect(string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var source = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[source.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, source, ciphertext, tag, AssociatedData);

        var payload = new byte[nonce.Length + tag.Length + ciphertext.Length];
        nonce.CopyTo(payload, 0);
        tag.CopyTo(payload, nonce.Length);
        ciphertext.CopyTo(payload, nonce.Length + tag.Length);
        return Prefix + Convert.ToBase64String(payload);
    }

    public string Unprotect(string protectedValue)
    {
        // Existing plaintext rows remain readable during migration; every newly written value is protected.
        if (!protectedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return protectedValue;
        }

        var payload = Convert.FromBase64String(protectedValue[Prefix.Length..]);
        if (payload.Length < 28)
        {
            throw new CryptographicException("Sensitive data payload is invalid.");
        }

        var nonce = payload.AsSpan(0, 12);
        var tag = payload.AsSpan(12, 16);
        var ciphertext = payload.AsSpan(28);
        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, AssociatedData);
        return Encoding.UTF8.GetString(plaintext);
    }
}

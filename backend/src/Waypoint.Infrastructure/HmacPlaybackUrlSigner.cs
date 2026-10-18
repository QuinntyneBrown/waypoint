using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

/// <summary>
/// Token = base64url(<c>expiryUnixSeconds|mediaId|keyId</c>) + "." + base64url(HMAC-SHA256 of that
/// payload). The token carries everything needed to validate it, so no database is consulted.
/// </summary>
public sealed class HmacPlaybackUrlSigner : IPlaybackUrlSigner
{
    private const string KeyId = "1";

    private readonly byte[] _key;
    private readonly TimeSpan _lifetime;
    private readonly IClock _clock;

    public HmacPlaybackUrlSigner(IOptions<MediaOptions> options, IClock clock)
    {
        _key = Encoding.UTF8.GetBytes(options.Value.PlaybackSigningKey);
        _lifetime = options.Value.PlaybackTokenLifetime;
        _clock = clock;
    }

    public SignedPlayback Sign(Guid mediaId)
    {
        var expiresAt = _clock.UtcNow.Add(_lifetime);
        var expiry = new DateTimeOffset(DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc)).ToUnixTimeSeconds();
        var payload = Encoding.UTF8.GetBytes($"{expiry}|{mediaId:N}|{KeyId}");
        var signature = HMACSHA256.HashData(_key, payload);
        return new SignedPlayback(
            $"{Base64Url.EncodeToString(payload)}.{Base64Url.EncodeToString(signature)}", expiresAt);
    }

    public bool TryValidate(string token, Guid mediaId)
    {
        var parts = token.Split('.');
        if (parts.Length != 2) return false;
        try
        {
            var payload = Base64Url.DecodeFromChars(parts[0]);
            var signature = Base64Url.DecodeFromChars(parts[1]);
            if (!CryptographicOperations.FixedTimeEquals(HMACSHA256.HashData(_key, payload), signature))
                return false;

            var fields = Encoding.UTF8.GetString(payload).Split('|');
            return fields.Length == 3
                && fields[2] == KeyId
                && Guid.TryParseExact(fields[1], "N", out var tokenMediaId)
                && tokenMediaId == mediaId
                && long.TryParse(fields[0], out var expiry)
                && DateTimeOffset.FromUnixTimeSeconds(expiry).UtcDateTime > _clock.UtcNow;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

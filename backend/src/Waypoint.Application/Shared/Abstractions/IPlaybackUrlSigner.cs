namespace Waypoint.Application.Shared.Abstractions;

public sealed record SignedPlayback(string Token, DateTime ExpiresAt);

/// <summary>Issues and checks the expiring tokens that authorize playback of one lesson video.</summary>
public interface IPlaybackUrlSigner
{
    SignedPlayback Sign(Guid mediaId);

    /// <summary>True only for an unexpired token that this signer issued for exactly this media.</summary>
    bool TryValidate(string token, Guid mediaId);
}

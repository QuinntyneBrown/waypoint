using System.Text.RegularExpressions;
using Microsoft.Net.Http.Headers;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Api;

/// <summary>
/// Serves a published lesson-video ladder. The signed token in the path is the only credential, so
/// the route is anonymous and never touches the database. Every rejection is a plain 404.
/// </summary>
public static partial class MediaDeliveryEndpoint
{
    private const string PlaylistCache = "private, max-age=300";
    private const string ImmutableCache = "private, max-age=31536000, immutable";

    // master.m3u8, poster.jpg, or one file inside a rendition folder: index.m3u8, init.mp4, seg_NNNNN.m4s.
    [GeneratedRegex(@"^(master\.m3u8|poster\.jpg|[a-z0-9]{3,8}/(index\.m3u8|init(_[0-9]{1,3})?\.mp4|seg_[0-9]{5}\.m4s))$")]
    private static partial Regex AllowedPath();

    public static void MapMediaDelivery(this IEndpointRouteBuilder app) =>
        app.MapGet("/api/media/{token}/{mediaId:guid}/{**path}", Serve).AllowAnonymous();

    private static IResult Serve(
        string token,
        Guid mediaId,
        string path,
        IPlaybackUrlSigner signer,
        IMediaStore store,
        HttpResponse response)
    {
        if (!AllowedPath().IsMatch(path) || !signer.TryValidate(token, mediaId))
        {
            return Results.NotFound();
        }

        var full = store.ResolvePublished(mediaId, path);
        if (full is null) return Results.NotFound();

        var playlist = path.EndsWith(".m3u8", StringComparison.Ordinal);
        response.Headers.CacheControl = playlist ? PlaylistCache : ImmutableCache;
        var info = new FileInfo(full);
        return Results.File(
            full,
            ContentType(path),
            lastModified: info.LastWriteTimeUtc,
            entityTag: new EntityTagHeaderValue($"\"{info.Length:x}-{info.LastWriteTimeUtc.Ticks:x}\""),
            enableRangeProcessing: true);
    }

    private static string ContentType(string path) => Path.GetExtension(path) switch
    {
        ".m3u8" => "application/vnd.apple.mpegurl",
        ".m4s" => "video/iso.segment",
        ".mp4" => "video/mp4",
        _ => "image/jpeg",
    };
}

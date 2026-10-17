using System.Text;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Api.TestSupport;

/// <summary>
/// Testing-only stand-in for ffprobe and ffmpeg, so acceptance tests run without the binaries.
/// A source whose head contains <c>NOTVIDEO</c> is rejected by the probe, and one containing
/// <c>FAILTRANSCODE</c> fails to transcode. Any other source probes as a 120 second, 1280x720 video
/// and transcodes to a small synthetic ladder (one 360p rendition, two segments and a poster).
/// </summary>
public sealed class FakeVideoTranscoder : IVideoTranscoder
{
    public async Task<VideoProbe?> ProbeAsync(string sourcePath, CancellationToken cancellationToken) =>
        (await ReadHeadAsync(sourcePath, cancellationToken)).Contains("NOTVIDEO", StringComparison.Ordinal)
            ? null
            : new VideoProbe(120, 1280, 720);

    public async Task TranscodeAsync(
        string sourcePath, string workDirectory, IProgress<double> progress, CancellationToken cancellationToken)
    {
        if ((await ReadHeadAsync(sourcePath, cancellationToken)).Contains("FAILTRANSCODE", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The fake transcoder was told to fail.");
        }

        var rendition = Path.Combine(workDirectory, "360p");
        Directory.CreateDirectory(rendition);
        await File.WriteAllTextAsync(
            Path.Combine(workDirectory, "master.m3u8"),
            "#EXTM3U\n#EXT-X-VERSION:7\n#EXT-X-STREAM-INF:BANDWIDTH=928000,RESOLUTION=640x360\n360p/index.m3u8\n",
            cancellationToken);
        await File.WriteAllTextAsync(
            Path.Combine(rendition, "index.m3u8"),
            "#EXTM3U\n#EXT-X-VERSION:7\n#EXT-X-TARGETDURATION:6\n#EXT-X-PLAYLIST-TYPE:VOD\n"
            + "#EXT-X-MAP:URI=\"init.mp4\"\n#EXTINF:6.0,\nseg_00001.m4s\n#EXTINF:6.0,\nseg_00002.m4s\n#EXT-X-ENDLIST\n",
            cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(rendition, "init.mp4"), Encoding.ASCII.GetBytes("\0\0\0\u0010ftypiso6\0\0\0\0"), cancellationToken);
        // Deterministic bytes so tests can assert on range requests.
        await File.WriteAllBytesAsync(Path.Combine(rendition, "seg_00001.m4s"), Enumerable.Range(0, 4096).Select(n => (byte)(n % 251)).ToArray(), cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(rendition, "seg_00002.m4s"), Enumerable.Range(0, 2048).Select(n => (byte)(n % 241)).ToArray(), cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(workDirectory, "poster.jpg"), [0xFF, 0xD8, 0xFF, 0xD9], cancellationToken);
        progress.Report(1);
    }

    private static async Task<string> ReadHeadAsync(string path, CancellationToken cancellationToken)
    {
        await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var head = new byte[64];
        var read = await file.ReadAsync(head, cancellationToken);
        return Encoding.ASCII.GetString(head, 0, read);
    }
}

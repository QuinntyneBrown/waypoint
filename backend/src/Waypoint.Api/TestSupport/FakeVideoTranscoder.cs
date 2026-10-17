using System.Text;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Api.TestSupport;

/// <summary>
/// Testing-only stand-in for ffprobe and ffmpeg, so acceptance tests run without the binaries.
/// A file whose head contains <c>NOTVIDEO</c> is rejected by the probe; any other file probes as a
/// 120 second, 1280x720 video.
/// </summary>
public sealed class FakeVideoTranscoder : IVideoTranscoder
{
    public async Task<VideoProbe?> ProbeAsync(string sourcePath, CancellationToken cancellationToken)
    {
        await using var file = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var head = new byte[64];
        var read = await file.ReadAsync(head, cancellationToken);
        return Encoding.ASCII.GetString(head, 0, read).Contains("NOTVIDEO", StringComparison.Ordinal)
            ? null
            : new VideoProbe(120, 1280, 720);
    }
}

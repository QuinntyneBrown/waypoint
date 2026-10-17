namespace Waypoint.Application.Shared.Abstractions;

/// <summary>What a probe learns about a source video.</summary>
public sealed record VideoProbe(double DurationSeconds, int Width, int Height);

/// <summary>Probes and transcodes video with the self-hosted ffprobe and ffmpeg binaries.</summary>
public interface IVideoTranscoder
{
    /// <summary>Returns null when the file is not a decodable video.</summary>
    Task<VideoProbe?> ProbeAsync(string sourcePath, CancellationToken cancellationToken);
}

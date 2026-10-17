namespace Waypoint.Application.Shared.Abstractions;

/// <summary>What a probe learns about a source video (size is the displayed size after rotation).</summary>
public sealed record VideoProbe(double DurationSeconds, int Width, int Height, bool HasAudio = true);

/// <summary>Probes and transcodes video with the self-hosted ffprobe and ffmpeg binaries.</summary>
public interface IVideoTranscoder
{
    /// <summary>Returns null when the file is not a decodable video.</summary>
    Task<VideoProbe?> ProbeAsync(string sourcePath, CancellationToken cancellationToken);

    /// <summary>
    /// Writes the HLS rendition ladder (<c>master.m3u8</c>, one folder per rendition) and
    /// <c>poster.jpg</c> into <paramref name="workDirectory"/>, reporting progress from 0 to 1.
    /// </summary>
    Task TranscodeAsync(
        string sourcePath,
        string workDirectory,
        IProgress<double> progress,
        CancellationToken cancellationToken);
}

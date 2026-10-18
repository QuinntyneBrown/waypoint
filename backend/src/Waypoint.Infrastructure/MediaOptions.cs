using System.ComponentModel.DataAnnotations;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public sealed class MediaOptions : IMediaSettings
{
    public bool Enabled { get; set; }

    [MaxLength(1024)]
    public string RootPath { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string FfmpegPath { get; set; } = "ffmpeg";

    [MaxLength(1024)]
    public string FfprobePath { get; set; } = "ffprobe";

    /// <summary>Upper bound for one source file; sized for a 3-hour camera-native recording.</summary>
    [Range(1, 200L * 1024 * 1024 * 1024)]
    public long MaxSourceBytes { get; set; } = 20L * 1024 * 1024 * 1024;

    [Range(1024 * 1024, 16 * 1024 * 1024)]
    public int ChunkSizeBytes { get; set; } = 8 * 1024 * 1024;

    [Range(1, 8)]
    public int TranscodeConcurrency { get; set; } = 1;

    public TimeSpan UploadSessionLifetime { get; set; } = TimeSpan.FromHours(24);

    [MaxLength(512)]
    public string PlaybackSigningKey { get; set; } = string.Empty;

    public TimeSpan PlaybackTokenLifetime { get; set; } = TimeSpan.FromHours(8);
}

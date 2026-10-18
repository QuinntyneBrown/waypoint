namespace Waypoint.Application.Shared.Abstractions;

/// <summary>Configured limits for self-hosted lesson video (the <c>Media</c> settings section).</summary>
public interface IMediaSettings
{
    bool Enabled { get; }
    long MaxSourceBytes { get; }
    int ChunkSizeBytes { get; }
    TimeSpan UploadSessionLifetime { get; }
}

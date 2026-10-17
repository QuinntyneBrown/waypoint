namespace Waypoint.Application.Shared.Abstractions;

/// <summary>Random-access storage for in-progress chunked uploads.</summary>
public interface IUploadStore
{
    /// <summary>Writes exactly <paramref name="length"/> bytes from the body at the offset.</summary>
    Task WriteChunkAsync(
        Guid uploadId, long offset, Stream body, long length, CancellationToken cancellationToken);

    /// <summary>Size in bytes of the assembled upload; 0 when nothing was written.</summary>
    long Length(Guid uploadId);

    /// <summary>Reads up to <paramref name="count"/> bytes from the start of the upload.</summary>
    Task<byte[]> ReadHeadAsync(Guid uploadId, int count, CancellationToken cancellationToken);

    /// <summary>Path of the assembled upload, for probing.</summary>
    string PartPath(Guid uploadId);

    /// <summary>Moves the assembled upload to <c>sources/{mediaId}.mp4</c>.</summary>
    void PromoteToSource(Guid uploadId, Guid mediaId);
}

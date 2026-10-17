using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

/// <summary>
/// The media root on disk: <c>uploads/</c> in-progress uploads, <c>sources/</c> accepted originals,
/// <c>work/</c> in-progress transcodes, <c>hls/</c> published renditions and <c>trash/</c>.
/// </summary>
public sealed class FileSystemMediaStore : IMediaStore, IUploadStore
{
    private readonly string _root;

    public FileSystemMediaStore(IOptions<MediaOptions> options)
    {
        _root = options.Value.RootPath;
    }

    public long AvailableFreeBytes()
    {
        Directory.CreateDirectory(_root);
        return new DriveInfo(Path.GetPathRoot(Path.GetFullPath(_root))!).AvailableFreeSpace;
    }

    public async Task WriteChunkAsync(
        Guid uploadId, long offset, Stream body, long length, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.Combine(_root, "uploads"));
        await using var file = new FileStream(
            PartPath(uploadId),
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.ReadWrite,
            bufferSize: 81920,
            useAsync: true);
        file.Seek(offset, SeekOrigin.Begin);
        var buffer = new byte[81920];
        long remaining = length;
        while (remaining > 0)
        {
            var read = await body.ReadAsync(
                buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)), cancellationToken);
            if (read == 0) break;
            await file.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            remaining -= read;
        }
        if (remaining != 0)
        {
            throw new InvalidDataException("The chunk body ended before its declared length.");
        }
    }

    public long Length(Guid uploadId)
    {
        var info = new FileInfo(PartPath(uploadId));
        return info.Exists ? info.Length : 0;
    }

    public async Task<byte[]> ReadHeadAsync(Guid uploadId, int count, CancellationToken cancellationToken)
    {
        await using var file = new FileStream(
            PartPath(uploadId), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var buffer = new byte[count];
        var read = await file.ReadAsync(buffer, cancellationToken);
        return buffer[..read];
    }

    public string PartPath(Guid uploadId) => Path.Combine(_root, "uploads", $"{uploadId:N}.part");

    public void PromoteToSource(Guid uploadId, Guid mediaId)
    {
        var sources = Path.Combine(_root, "sources");
        Directory.CreateDirectory(sources);
        File.Move(PartPath(uploadId), Path.Combine(sources, $"{mediaId:N}.mp4"));
    }
}

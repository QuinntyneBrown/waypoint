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

    public string SourcePath(Guid mediaId) => Path.Combine(_root, "sources", $"{mediaId:N}.mp4");

    public bool SourceExists(Guid mediaId) => File.Exists(SourcePath(mediaId));

    public string PrepareWorkDirectory(Guid mediaId)
    {
        var directory = Folder("work", mediaId);
        if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        Directory.CreateDirectory(directory);
        return directory;
    }

    public void DiscardWork(Guid mediaId)
    {
        var directory = Folder("work", mediaId);
        if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
    }

    public void Publish(Guid mediaId)
    {
        var published = Folder("hls", mediaId);
        Directory.CreateDirectory(Path.Combine(_root, "hls"));
        if (Directory.Exists(published)) Directory.Delete(published, recursive: true);
        Directory.Move(Folder("work", mediaId), published);
    }

    public void MoveToTrash(Guid mediaId)
    {
        var trash = Folder("trash", mediaId);
        Directory.CreateDirectory(trash);
        var published = Folder("hls", mediaId);
        if (Directory.Exists(published)) Directory.Move(published, Path.Combine(trash, "hls"));
        if (SourceExists(mediaId)) File.Move(SourcePath(mediaId), Path.Combine(trash, "source.mp4"));
        DiscardWork(mediaId);
    }

    public void Sweep(
        IReadOnlySet<Guid> existingMediaIds, IReadOnlySet<Guid> transcodingMediaIds, DateTime partCutoff)
    {
        DeleteChildren(Path.Combine(_root, "trash"), _ => true);
        DeleteChildren(Path.Combine(_root, "work"),
            name => !(Guid.TryParseExact(name, "N", out var id) && transcodingMediaIds.Contains(id)));
        DeleteChildren(Path.Combine(_root, "hls"),
            name => !(Guid.TryParseExact(name, "N", out var id) && existingMediaIds.Contains(id)));
        var uploads = Path.Combine(_root, "uploads");
        if (!Directory.Exists(uploads)) return;
        foreach (var part in Directory.EnumerateFiles(uploads, "*.part"))
        {
            if (File.GetLastWriteTimeUtc(part) < partCutoff) File.Delete(part);
        }
    }

    private string Folder(string area, Guid mediaId) => Path.Combine(_root, area, $"{mediaId:N}");

    private static void DeleteChildren(string directory, Func<string, bool> shouldDelete)
    {
        if (!Directory.Exists(directory)) return;
        foreach (var child in Directory.EnumerateDirectories(directory))
        {
            if (shouldDelete(Path.GetFileName(child))) Directory.Delete(child, recursive: true);
        }
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

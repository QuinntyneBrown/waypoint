namespace Waypoint.Application.Shared.Abstractions;

/// <summary>
/// The media root on disk: <c>uploads/</c> in-progress uploads, <c>sources/</c> accepted originals,
/// <c>work/</c> in-progress transcodes, <c>hls/</c> published renditions and <c>trash/</c>.
/// </summary>
public interface IMediaStore
{
    long AvailableFreeBytes();

    string SourcePath(Guid mediaId);

    bool SourceExists(Guid mediaId);

    /// <summary>Returns an empty <c>work/{mediaId}</c> folder, replacing any earlier one.</summary>
    string PrepareWorkDirectory(Guid mediaId);

    void DiscardWork(Guid mediaId);

    /// <summary>Moves <c>work/{mediaId}</c> to <c>hls/{mediaId}</c> in one step.</summary>
    void Publish(Guid mediaId);

    /// <summary>Moves the published ladder and the source to <c>trash/{mediaId}</c>.</summary>
    void MoveToTrash(Guid mediaId);

    /// <summary>
    /// Purges <c>trash/</c>, uploads untouched since <paramref name="partCutoff"/>, work folders of
    /// media that are not being transcoded, and published folders whose media no longer exists.
    /// </summary>
    void Sweep(
        IReadOnlySet<Guid> existingMediaIds,
        IReadOnlySet<Guid> transcodingMediaIds,
        DateTime partCutoff);
}

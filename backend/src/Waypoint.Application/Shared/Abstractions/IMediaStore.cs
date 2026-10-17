namespace Waypoint.Application.Shared.Abstractions;

/// <summary>The media root on disk: uploads, sources, work, published renditions and trash.</summary>
public interface IMediaStore
{
    long AvailableFreeBytes();
}

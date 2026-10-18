namespace Waypoint.Domain.CurriculumAuthoring;

/// <summary>Where a lesson video is in the self-hosted streaming pipeline.</summary>
public enum VideoStatus
{
    /// <summary>Not a streamed video (a document, or an older stored or external video).</summary>
    None = 0,
    Queued = 1,
    Processing = 2,
    Ready = 3,
    Failed = 4,
}

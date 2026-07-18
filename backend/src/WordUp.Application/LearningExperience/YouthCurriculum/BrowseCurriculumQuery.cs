using MediatR;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

/// <summary>A youth browses the published modules of a track with per-module status + gating (L2-LEARN-001).</summary>
public record BrowseCurriculumQuery(Guid? TrackId = null) : IRequest<CurriculumView>;

public record CurriculumView(
    Guid SelectedTrackId,
    IReadOnlyList<TrackOption> Tracks,
    IReadOnlyList<ModuleStatus> Modules);

public record TrackOption(Guid TrackId, string Name, string? Description, bool Selected);

public record ModuleStatus(
    Guid ModuleId,
    string Name,
    int Order,
    string Status,
    double CompletionPercent,
    bool Locked,
    string? UnlockCondition,
    string? Description,
    string? ScriptureAnchor,
    IReadOnlyList<CurriculumItem> Items,
    int CompletedItems,
    int TotalItems,
    bool Current);

public record CurriculumItem(
    Guid ItemId,
    string Type,
    string Title,
    int Order,
    string Status,
    bool Locked,
    string? UnlockCondition,
    int? EstimatedMinutes);

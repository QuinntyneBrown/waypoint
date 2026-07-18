using MediatR;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

/// <summary>A youth browses the published modules of a track with per-module status + gating (L2-LEARN-001).</summary>
public record BrowseCurriculumQuery(Guid TrackId) : IRequest<CurriculumView>;

public record CurriculumView(IReadOnlyList<ModuleStatus> Modules);

public record ModuleStatus(
    Guid ModuleId,
    string Name,
    int Order,
    string Status,
    double CompletionPercent,
    bool Locked,
    string? UnlockCondition);

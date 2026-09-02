using MediatR;

namespace Waypoint.Application.LearningExperience.YouthCurriculum;

/// <summary>A youth's journey map: module progress + earned/locked badges (L2-LEARN-009).</summary>
public record JourneyQuery : IRequest<JourneyView>;

public record JourneyView(
    IReadOnlyList<ModuleStatus> Modules,
    IReadOnlyList<EarnedBadge> Earned,
    IReadOnlyList<LockedBadge> Locked,
    IReadOnlyList<JourneyMilestone> Milestones);

public record EarnedBadge(Guid BadgeId, string Name, string Icon, string Criteria, DateTime AwardedAt);

public record LockedBadge(
    Guid BadgeId,
    string Name,
    string Icon,
    string Criteria,
    int CompletedCriteria,
    int TotalCriteria,
    string ProgressLabel);

public record JourneyMilestone(
    string Type,
    string Title,
    string Detail,
    DateTime? OccurredAt,
    bool Current);

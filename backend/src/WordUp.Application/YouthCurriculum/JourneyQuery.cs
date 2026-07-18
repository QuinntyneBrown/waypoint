using MediatR;

namespace WordUp.Application.YouthCurriculum;

/// <summary>A youth's journey map: module progress + earned/locked badges (L2-LEARN-009).</summary>
public record JourneyQuery : IRequest<JourneyView>;

public record JourneyView(
    IReadOnlyList<ModuleStatus> Modules,
    IReadOnlyList<EarnedBadge> Earned,
    IReadOnlyList<LockedBadge> Locked);

public record EarnedBadge(Guid BadgeId, string Name, string Icon, DateTime AwardedAt);

public record LockedBadge(Guid BadgeId, string Name, string Icon, string Criteria);

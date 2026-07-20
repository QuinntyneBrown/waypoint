using MediatR;

namespace WordUp.Application.Mentoring.Flags;

/// <summary>Active at-risk flags for the current mentor's assigned youth (L2-MENTOR-007).</summary>
public record AtRiskListQuery() : IRequest<IReadOnlyList<AtRiskYouth>>, IAtomicOutboxRequest;

public record AtRiskYouth(
    Guid FlagId,
    Guid YouthId,
    string DisplayName,
    IReadOnlyList<string> Reasons,
    DateTime FlaggedAt,
    string FlaggedByName,
    bool Automatic);

using MediatR;

namespace WordUp.Application.Mentoring.Flags;

/// <summary>Active at-risk flags for the current mentor's assigned youth (L2-MENTOR-007).</summary>
public record AtRiskListQuery() : IRequest<IReadOnlyList<AtRiskYouth>>;

public record AtRiskYouth(Guid YouthId, string DisplayName, string Reason);

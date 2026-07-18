using MediatR;

namespace WordUp.Application.MentorRoster;

/// <summary>The current mentor's assigned youth, optionally filtered/searched (L2-MENTOR-001 AC2/AC4, L2-AUTH-010).</summary>
public record RosterQuery(string? Filter = null, string? Search = null) : IRequest<IReadOnlyList<RosterYouth>>;

public record RosterYouth(Guid Id, string DisplayName, string Status, bool AtRisk);

using MediatR;

namespace Waypoint.Application.Mentoring.Flags;

/// <summary>A mentor raises an at-risk flag on an assigned youth (L2-MENTOR-007).</summary>
public record RaiseFlagCommand(Guid YouthId, string Reason) : IRequest;

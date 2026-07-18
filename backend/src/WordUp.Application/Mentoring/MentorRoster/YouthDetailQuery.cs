using MediatR;

namespace WordUp.Application.Mentoring.MentorRoster;

/// <summary>Detail view of a single youth for the mentor, with assignment enforcement (L2-MENTOR-002, L2-AUTH-010).</summary>
public record YouthDetailQuery(Guid YouthId) : IRequest<YouthDetail>;

public record YouthDetail(
    Guid Id,
    string DisplayName,
    string Email,
    Guid? CohortId,
    Guid? GuardianUserId,
    bool AtRisk,
    string? AtRiskReason,
    Guid? FlaggedBy,
    DateTime? FlaggedAt);

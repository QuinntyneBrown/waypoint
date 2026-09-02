using MediatR;

namespace Waypoint.Application.Mentoring.CheckIns;

public record CheckInHistoryQuery(Guid? YouthId, string? Type) : IRequest<IReadOnlyList<CheckInSummary>>;

public record CheckInSummary(
    Guid Id,
    Guid YouthUserId,
    string YouthName,
    DateTime Date,
    string Type,
    string? Engagement,
    string? GoalsDiscussed,
    string? PrayerRequests,
    string? ActionItems,
    string Visibility,
    DateTime? NextFollowUp,
    bool GuardianInformed,
    DateTime CreatedAt);

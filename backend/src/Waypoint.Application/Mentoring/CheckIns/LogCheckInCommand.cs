using MediatR;

namespace Waypoint.Application.Mentoring.CheckIns;

public record LogCheckInCommand(
    Guid YouthId,
    DateTime Date,
    string Type,
    string? Engagement,
    string? GoalsDiscussed,
    string? PrayerRequests,
    string? ActionItems,
    DateTime? NextFollowUp,
    bool ShareWithYouthAndGuardian,
    bool GuardianInformed) : IRequest<CheckInResult>;

public record CheckInResult(Guid Id);

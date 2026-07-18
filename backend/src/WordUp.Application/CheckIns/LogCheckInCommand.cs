using MediatR;

namespace WordUp.Application.CheckIns;

public record LogCheckInCommand(
    Guid YouthId,
    DateTime Date,
    string Type,
    string? Engagement,
    string? GoalsDiscussed,
    string? PrayerRequests,
    string? ActionItems,
    DateTime? NextFollowUp,
    bool ShareWithMentor,
    bool GuardianInformed) : IRequest<CheckInResult>;

public record CheckInResult(Guid Id);

namespace WordUp.Api.Contracts.Mentoring;

public record LogCheckInRequest(
    Guid YouthId,
    DateTime Date,
    string Type,
    string? Engagement,
    string? GoalsDiscussed,
    string? PrayerRequests,
    string? ActionItems,
    DateTime? NextFollowUp,
    bool ShareWithYouthAndGuardian,
    bool GuardianInformed);

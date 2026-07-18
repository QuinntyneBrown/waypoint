using MediatR;

namespace WordUp.Application.CheckIns;

public record CheckInHistoryQuery(Guid? YouthId, string? Type) : IRequest<IReadOnlyList<CheckInSummary>>;

// Deliberately excludes PrayerRequests and other note content — mentor-only fields (MENTOR-006 AC3/AC5).
public record CheckInSummary(
    Guid Id,
    Guid YouthUserId,
    DateTime Date,
    string Type,
    string Visibility,
    DateTime? NextFollowUp);

using MediatR;
using WordUp.Application.LearningExperience.YouthCurriculum;

namespace WordUp.Application.GuardianPortal;

/// <summary>A guardian views their linked child's read-only aggregate: overall + per-module progress,
/// attendance, earned/locked badges, and upcoming schedule. Never lesson content, quiz answers,
/// private reflections, or mentor private notes (L2-GUARDIAN-001 AC1/AC2, L2-AUTH-010).</summary>
public record ChildProgressQuery(Guid ChildId) : IRequest<ChildProgressView>;

public record ChildProgressView(
    Guid ChildId,
    string ChildName,
    double OverallPercent,
    IReadOnlyList<ModuleStatus> Modules,
    ChildAttendanceSummary Attendance,
    IReadOnlyList<ChildAttendanceItem> AttendanceHistory,
    IReadOnlyList<ChildBadge> EarnedBadges,
    IReadOnlyList<ChildBadge> LockedBadges,
    IReadOnlyList<ChildUpcomingSession> UpcomingSessions,
    IReadOnlyList<GuardianSharedCheckIn> SharedCheckIns);

public record ChildAttendanceSummary(
    int Present,
    int Late,
    int Excused,
    int Absent,
    int Total,
    double Rate);

public record ChildBadge(Guid BadgeId, string Name, string Icon, string? Description);

public record ChildUpcomingSession(Guid SessionId, string Title, DateTime StartsAt, string? Location);

public record ChildAttendanceItem(
    Guid SessionId,
    string Title,
    DateTime StartsAt,
    string? Location,
    string Status);

public record GuardianSharedCheckIn(
    Guid CheckInId,
    DateTime Date,
    string Type,
    string? Engagement,
    string? GoalsDiscussed,
    string? PrayerRequests,
    string? ActionItems,
    DateTime? NextFollowUp);

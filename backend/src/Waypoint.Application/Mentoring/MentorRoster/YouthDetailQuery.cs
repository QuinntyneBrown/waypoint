using MediatR;

namespace Waypoint.Application.Mentoring.MentorRoster;

/// <summary>Detail view of a single youth for the mentor, with assignment enforcement (L2-MENTOR-002, L2-AUTH-010).</summary>
public record YouthDetailQuery(Guid YouthId) : IRequest<YouthDetail>, IAtomicOutboxRequest;

public record YouthDetail(
    Guid Id,
    string DisplayName,
    string Email,
    string? Pronouns,
    string? PhotoUrl,
    Guid? CohortId,
    string? CohortName,
    Guid? GuardianUserId,
    YouthGuardianContact? Guardian,
    bool AtRisk,
    string? AtRiskReason,
    Guid? FlaggedBy,
    string? FlaggedByName,
    DateTime? FlaggedAt,
    YouthProgressSummary Progress,
    IReadOnlyList<YouthAttendanceSummary> Attendance,
    IReadOnlyList<YouthCheckInSummary> CheckIns,
    IReadOnlyList<YouthSubmissionSummary> Submissions,
    IReadOnlyList<YouthCompetencySummary> Competencies);

public record YouthGuardianContact(
    string Name,
    string Email,
    string? Phone,
    string? EmergencyContactName,
    string? EmergencyContactPhone);

public record YouthProgressSummary(int CompletedLessons, int TotalLessons, double Percent);

public record YouthAttendanceSummary(
    Guid SessionId,
    string SessionTitle,
    DateTime StartsAt,
    string Status);

public record YouthCheckInSummary(
    Guid Id,
    DateTime Date,
    string Type,
    string Visibility,
    DateTime? NextFollowUp);

public record YouthSubmissionSummary(
    Guid Id,
    string AssignmentTitle,
    string Status,
    DateTime SubmittedAt);

public record YouthCompetencySummary(
    Guid CompetencyId,
    string Name,
    string Level,
    string? Notes,
    DateTime AssessedAt);

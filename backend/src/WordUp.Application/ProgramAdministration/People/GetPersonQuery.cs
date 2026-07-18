using MediatR;

namespace WordUp.Application.ProgramAdministration.People;

public record GetPersonQuery(Guid Id) : IRequest<PersonDetail>;

public record PersonDetail(
    Guid Id,
    string DisplayName,
    string Email,
    string Role,
    string Status,
    string? Pronouns,
    string? PhotoUrl,
    bool OnboardingCompleted,
    DateTime CreatedAt,
    DateTime? LastActive,
    Guid? CohortId,
    string? CohortName,
    Guid? MentorId,
    string? MentorName,
    Guid? GuardianUserId,
    PersonGuardianContact? Guardian,
    bool HasValidConsent,
    PersonConsentSummary Consent,
    PersonProgressSummary Progress,
    PersonAttendanceSummary Attendance,
    IReadOnlyList<PersonNoteSummary> Notes);

public record PersonGuardianContact(
    string Name,
    string Email,
    string? Relationship,
    string? Phone,
    string? EmergencyContactName,
    string? EmergencyContactPhone);

public record PersonConsentSummary(
    bool HasValidParticipationConsent,
    bool ParticipationConsent,
    bool MediaConsent,
    DateTime? SignedAt,
    DateTime? ValidUntil,
    DateTime? WithdrawnAt,
    string? GuardianName,
    string? Relationship);

public record PersonProgressSummary(int CompletedLessons, int TotalLessons, double Percent);

public record PersonAttendanceSummary(
    int Attended,
    int Total,
    double Rate,
    IReadOnlyList<PersonAttendanceItem> Items);

public record PersonAttendanceItem(
    Guid SessionId,
    string SessionTitle,
    DateTime StartsAt,
    string Status,
    string? Note);

public record PersonNoteSummary(
    Guid Id,
    DateTime Date,
    string Type,
    string MentorName,
    string Summary,
    DateTime? NextFollowUp);

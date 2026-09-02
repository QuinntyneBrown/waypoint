using MediatR;

namespace Waypoint.Application.ApplicationIntake;

public sealed record ApplicationDetailQuery(Guid ApplicationId) : IRequest<ApplicationDetailView>;

public sealed record ApplicationDetailView(
    Guid Id,
    string FirstName,
    string LastName,
    int Age,
    bool IsMinor,
    string WhoIsFilling,
    string Neighbourhood,
    string CohortInterest,
    string WhyInterested,
    string? TrackInterest,
    string? GuardianName,
    string? GuardianEmail,
    string? GuardianPhone,
    bool ParticipationConsentAck,
    bool PrivacyAck,
    string Status,
    string ConsentState,
    bool CanApprove,
    Guid? CohortId,
    Guid? MentorId,
    string? DecisionNote,
    DateTime CreatedAt,
    DateTime? ReviewedAt);

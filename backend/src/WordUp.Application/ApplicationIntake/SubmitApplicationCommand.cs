using MediatR;

namespace WordUp.Application.ApplicationIntake;

public record SubmitApplicationCommand(
    string FirstName,
    string LastName,
    int Age,
    string Neighbourhood,
    string WhyInterested,
    string? TrackInterest,
    string? GuardianName,
    string? GuardianEmail,
    string? GuardianPhone,
    bool ParticipationConsentAck,
    bool PrivacyAck) : IRequest<SubmitApplicationResult>;

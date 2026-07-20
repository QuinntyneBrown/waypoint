using MediatR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ApplicationIntake;

public record SubmitApplicationCommand(
    string FirstName,
    string LastName,
    int Age,
    string WhoIsFilling,
    string Neighbourhood,
    string CohortInterest,
    string WhyInterested,
    string? TrackInterest,
    string? GuardianName,
    string? GuardianEmail,
    string? GuardianPhone,
    bool ParticipationConsentAck,
    bool PrivacyAck) : IRequest<SubmitApplicationResult>, IAtomicRequest;

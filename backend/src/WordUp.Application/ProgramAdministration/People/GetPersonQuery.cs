using MediatR;

namespace WordUp.Application.ProgramAdministration.People;

public record GetPersonQuery(Guid Id) : IRequest<PersonDetail>;

public record PersonDetail(
    Guid Id,
    string DisplayName,
    string Email,
    string Role,
    string Status,
    Guid? CohortId,
    Guid? MentorId,
    Guid? GuardianUserId,
    bool HasValidConsent);

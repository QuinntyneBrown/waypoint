using MediatR;

namespace WordUp.Application.People;

public record ListPeopleQuery(
    int Page = 1,
    int PageSize = 50,
    string? Role = null,
    Guid? CohortId = null,
    string? Search = null) : IRequest<PagedPeopleResult>;

public record PagedPeopleResult(IReadOnlyList<PersonSummary> Items, int Total, int Page, int PageSize);

public record PersonSummary(
    Guid Id,
    string DisplayName,
    string Email,
    string Role,
    string Status,
    Guid? CohortId,
    Guid? MentorId,
    DateTime? LastActive);

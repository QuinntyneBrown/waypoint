using MediatR;

namespace Waypoint.Application.Mentoring.AssignWork;

public record AssignWorkOptionsQuery : IRequest<AssignWorkOptions>;

public record AssignWorkOptions(
    IReadOnlyList<AssignableWorkItem> Items,
    IReadOnlyList<AssignableYouth> Youth,
    IReadOnlyList<AssignableCohort> Cohorts);

public record AssignableWorkItem(
    Guid ItemId,
    string ItemType,
    string Title,
    string ModuleName);

public record AssignableYouth(
    Guid YouthId,
    string DisplayName,
    Guid? CohortId,
    string? CohortName,
    bool AtRisk);

public record AssignableCohort(Guid CohortId, string Name, int YouthCount);

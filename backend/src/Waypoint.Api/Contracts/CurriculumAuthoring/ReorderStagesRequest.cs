namespace Waypoint.Api.Contracts.CurriculumAuthoring;

public record ReorderStagesRequest(IReadOnlyList<Guid> OrderedStageIds);

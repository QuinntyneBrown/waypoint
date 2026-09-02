namespace Waypoint.Api.Contracts.CurriculumAuthoring;

public record ReorderModulesRequest(Guid TrackId, IReadOnlyList<Guid> OrderedModuleIds);

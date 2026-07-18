namespace WordUp.Api.Contracts.CurriculumAuthoring;

public record ReorderModulesRequest(Guid TrackId, IReadOnlyList<Guid> OrderedModuleIds);

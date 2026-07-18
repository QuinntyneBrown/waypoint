namespace WordUp.Api.Contracts.CurriculumAuthoring;

public record ReorderStagesRequest(IReadOnlyList<Guid> OrderedStageIds);

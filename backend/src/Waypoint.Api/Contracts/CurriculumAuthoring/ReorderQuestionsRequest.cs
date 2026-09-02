namespace Waypoint.Api.Contracts.CurriculumAuthoring;

public sealed record ReorderQuestionsRequest(IReadOnlyList<Guid> OrderedQuestionIds);

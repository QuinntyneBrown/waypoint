namespace WordUp.Api.Contracts.CurriculumAuthoring;

public sealed record ReorderQuestionsRequest(IReadOnlyList<Guid> OrderedQuestionIds);

namespace WordUp.Api.Contracts.LearningExperience;

public sealed record SaveCodingProgressRequest(string Code, IReadOnlyList<Guid> CheckedStepIds);

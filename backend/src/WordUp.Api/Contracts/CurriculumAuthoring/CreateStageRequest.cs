namespace WordUp.Api.Contracts.CurriculumAuthoring;

public record CreateStageRequest(string Name, string? Description, int Order);

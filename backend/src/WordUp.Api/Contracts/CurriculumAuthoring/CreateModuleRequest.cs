namespace WordUp.Api.Contracts.CurriculumAuthoring;

public record CreateModuleRequest(
    Guid TrackId, string Name, string? Description, Guid? PrerequisiteModuleId, string? ScriptureAnchor);

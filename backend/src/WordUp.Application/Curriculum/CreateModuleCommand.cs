using MediatR;

namespace WordUp.Application.Curriculum;

public record CreateModuleCommand(
    Guid TrackId,
    string Name,
    string? Description,
    Guid? PrerequisiteModuleId,
    string? ScriptureAnchor) : IRequest<ModuleResult>;

public record ModuleResult(Guid Id);

using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public record CreateModuleCommand(
    Guid TrackId,
    string Name,
    string? Description,
    Guid? PrerequisiteModuleId,
    string? ScriptureAnchor) : IRequest<ModuleResult>;

public record ModuleResult(Guid Id);

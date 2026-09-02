using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public record ReorderModulesCommand(Guid TrackId, IReadOnlyList<Guid> OrderedModuleIds) : IRequest;

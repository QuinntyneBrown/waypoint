using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public record ReorderModulesCommand(Guid TrackId, IReadOnlyList<Guid> OrderedModuleIds) : IRequest;

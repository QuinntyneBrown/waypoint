using MediatR;

namespace WordUp.Application.Curriculum;

public record ReorderModulesCommand(Guid TrackId, IReadOnlyList<Guid> OrderedModuleIds) : IRequest;

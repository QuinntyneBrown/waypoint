using MediatR;

namespace WordUp.Application.Progression;

public record ReorderStagesCommand(IReadOnlyList<Guid> OrderedStageIds) : IRequest;

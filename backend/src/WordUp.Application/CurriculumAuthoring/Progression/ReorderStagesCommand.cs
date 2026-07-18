using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public record ReorderStagesCommand(IReadOnlyList<Guid> OrderedStageIds) : IRequest;

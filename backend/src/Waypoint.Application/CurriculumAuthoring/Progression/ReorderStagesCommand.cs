using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Progression;

public record ReorderStagesCommand(IReadOnlyList<Guid> OrderedStageIds) : IRequest;

using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record ReorderLessonsCommand(
    Guid ModuleId,
    IReadOnlyList<Guid> OrderedLessonIds) : IRequest;

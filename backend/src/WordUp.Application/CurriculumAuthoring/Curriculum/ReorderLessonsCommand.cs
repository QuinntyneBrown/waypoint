using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public sealed record ReorderLessonsCommand(
    Guid ModuleId,
    IReadOnlyList<Guid> OrderedLessonIds) : IRequest;

namespace Waypoint.Api.Contracts.CurriculumAuthoring;

public sealed record ReorderLessonsRequest(Guid ModuleId, IReadOnlyList<Guid> OrderedLessonIds);

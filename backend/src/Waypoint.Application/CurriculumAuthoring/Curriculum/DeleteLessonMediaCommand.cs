using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record DeleteLessonMediaCommand(Guid LessonId, Guid MediaId) : IRequest;

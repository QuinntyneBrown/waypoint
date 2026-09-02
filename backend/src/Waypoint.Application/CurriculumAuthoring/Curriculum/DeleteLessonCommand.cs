using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public record DeleteLessonCommand(Guid LessonId) : IRequest;

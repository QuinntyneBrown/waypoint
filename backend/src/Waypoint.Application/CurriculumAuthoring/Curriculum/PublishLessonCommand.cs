using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public record PublishLessonCommand(Guid LessonId, PublishStatus Status) : IRequest;

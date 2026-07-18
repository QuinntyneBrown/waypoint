using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public record PublishLessonCommand(Guid LessonId, PublishStatus Status) : IRequest;

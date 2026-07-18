using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public record DeleteLessonCommand(Guid LessonId) : IRequest;

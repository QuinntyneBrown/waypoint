using MediatR;

namespace WordUp.Application.Curriculum;

public record DeleteLessonCommand(Guid LessonId) : IRequest;

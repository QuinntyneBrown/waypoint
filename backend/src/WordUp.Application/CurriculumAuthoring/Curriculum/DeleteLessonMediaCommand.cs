using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public sealed record DeleteLessonMediaCommand(Guid LessonId, Guid MediaId) : IRequest;

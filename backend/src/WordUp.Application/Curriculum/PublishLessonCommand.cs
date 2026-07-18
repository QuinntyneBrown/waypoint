using MediatR;
using WordUp.Domain;

namespace WordUp.Application.Curriculum;

public record PublishLessonCommand(Guid LessonId, PublishStatus Status) : IRequest;

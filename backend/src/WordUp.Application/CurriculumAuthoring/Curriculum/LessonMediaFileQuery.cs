using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public sealed record LessonMediaFileQuery(Guid MediaId) : IRequest<LessonMediaFileView>;

public sealed record LessonMediaFileView(
    string FileName,
    string ContentType,
    string MediaType,
    byte[] Content);

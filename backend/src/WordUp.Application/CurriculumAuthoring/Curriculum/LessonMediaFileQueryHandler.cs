using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.LearningExperience.YouthCurriculum;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public sealed class LessonMediaFileQueryHandler
    : IRequestHandler<LessonMediaFileQuery, LessonMediaFileView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public LessonMediaFileQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<LessonMediaFileView> Handle(
        LessonMediaFileQuery request,
        CancellationToken cancellationToken)
    {
        var media = await _db.LessonMedia.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == request.MediaId && item.Content != null,
            cancellationToken) ?? throw new NotFoundException("Lesson media not found.");
        if (_currentUser.Role == Roles.Youth)
        {
            var youthId = _currentUser.UserId ?? throw new ForbiddenException();
            await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
                _db,
                media.LessonId,
                youthId,
                cancellationToken);
        }
        else if (_currentUser.Role is not (Roles.ProgramAdministrator or Roles.Mentor))
        {
            throw new ForbiddenException("This media is not available to your role.");
        }

        return new LessonMediaFileView(
            media.OriginalFileName ?? "lesson-media",
            media.ContentType ?? "application/octet-stream",
            media.MediaType,
            media.Content!);
    }
}

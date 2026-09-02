using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed class StoreLessonMediaCommandHandler
    : IRequestHandler<StoreLessonMediaCommand, LessonMediaResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public StoreLessonMediaCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<LessonMediaResult> Handle(
        StoreLessonMediaCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _db.Lessons.AnyAsync(
            item => item.Id == request.LessonId && item.ArchivedAt == null,
            cancellationToken))
        {
            throw new NotFoundException("Lesson not found.");
        }

        var media = new LessonMedia
        {
            LessonId = request.LessonId,
            MediaType = LessonMediaUploadPolicy.MediaType(request.ContentType),
            OriginalFileName = Path.GetFileName(request.FileName),
            ContentType = request.ContentType,
            SizeBytes = request.Content.LongLength,
            Content = request.Content,
            CreatedAt = _clock.UtcNow,
        };
        media.Url = $"/api/lesson-media/{media.Id}";
        _db.LessonMedia.Add(media);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "LessonMediaAdded",
            _currentUser.UserId,
            "LessonMedia",
            media.Id,
            cancellationToken: cancellationToken);
        return new LessonMediaResult(
            media.Id,
            media.MediaType,
            media.OriginalFileName,
            media.ContentType,
            media.SizeBytes,
            media.Url);
    }
}

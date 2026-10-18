using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;
using Waypoint.Domain.CurriculumAuthoring;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed class StartVideoUploadCommandHandler
    : IRequestHandler<StartVideoUploadCommand, VideoUploadSessionResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IMediaSettings _media;
    private readonly IMediaStore _store;

    public StartVideoUploadCommandHandler(
        IAppDbContext db, IClock clock, IMediaSettings media, IMediaStore store)
    {
        _db = db;
        _clock = clock;
        _media = media;
        _store = store;
    }

    public async Task<VideoUploadSessionResult> Handle(
        StartVideoUploadCommand request, CancellationToken cancellationToken)
    {
        if (!_media.Enabled
            || !await _db.Lessons.AnyAsync(
                item => item.Id == request.LessonId && item.ArchivedAt == null, cancellationToken))
        {
            throw new NotFoundException("Lesson not found.");
        }

        // The source plus the rendition ladder need about one and a half times the source size.
        if (_store.AvailableFreeBytes() < request.SizeBytes + request.SizeBytes / 2)
        {
            throw new ConflictException("There is not enough storage space for this video.");
        }

        var chunkSize = _media.ChunkSizeBytes;
        var chunkCount = (int)((request.SizeBytes + chunkSize - 1) / chunkSize);
        var now = _clock.UtcNow;
        var session = new VideoUploadSession
        {
            LessonId = request.LessonId,
            FileName = Path.GetFileName(request.FileName),
            ExpectedBytes = request.SizeBytes,
            ChunkSizeBytes = chunkSize,
            ChunkCount = chunkCount,
            ReceivedChunks = new byte[(chunkCount + 7) / 8],
            CreatedAt = now,
            ExpiresAt = now.Add(_media.UploadSessionLifetime),
        };
        _db.VideoUploadSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
        return new VideoUploadSessionResult(session.Id, chunkSize, chunkCount);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record VideoUploadStatusQuery(Guid LessonId, Guid UploadId)
    : IRequest<VideoUploadStatusResult>;

public sealed record VideoUploadStatusResult(
    Guid UploadId, int ChunkSizeBytes, int ChunkCount, IReadOnlyList<int> ReceivedChunks);

public sealed class VideoUploadStatusQueryHandler
    : IRequestHandler<VideoUploadStatusQuery, VideoUploadStatusResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public VideoUploadStatusQueryHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<VideoUploadStatusResult> Handle(
        VideoUploadStatusQuery request, CancellationToken cancellationToken)
    {
        var session = await _db.VideoUploadSessions.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == request.UploadId && item.LessonId == request.LessonId,
            cancellationToken);
        if (session is null || session.ExpiresAt <= _clock.UtcNow)
        {
            throw new NotFoundException("Upload not found.");
        }

        var received = Enumerable.Range(0, session.ChunkCount)
            .Where(index => (session.ReceivedChunks[index / 8] & (1 << (index % 8))) != 0)
            .ToList();
        return new VideoUploadStatusResult(
            session.Id, session.ChunkSizeBytes, session.ChunkCount, received);
    }
}

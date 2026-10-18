using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;
using Waypoint.Domain.CurriculumAuthoring;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record RetryVideoTranscodeCommand(Guid LessonId, Guid MediaId) : IRequest;

public sealed class RetryVideoTranscodeCommandHandler : IRequestHandler<RetryVideoTranscodeCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IMediaStore _store;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public RetryVideoTranscodeCommandHandler(
        IAppDbContext db, IClock clock, IMediaStore store, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _store = store;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(RetryVideoTranscodeCommand request, CancellationToken cancellationToken)
    {
        var media = await _db.LessonMedia.FirstOrDefaultAsync(
            item => item.Id == request.MediaId && item.LessonId == request.LessonId,
            cancellationToken) ?? throw new NotFoundException("Lesson media not found.");
        if (media.VideoStatus != VideoStatus.Failed)
        {
            throw new ConflictException("Only a failed video can be retried.");
        }
        if (!_store.SourceExists(media.Id))
        {
            throw new ConflictException("The original video file is no longer available.");
        }

        var job = await _db.VideoTranscodeJobs.FirstOrDefaultAsync(
            item => item.MediaId == media.Id, cancellationToken);
        if (job is null)
        {
            _db.VideoTranscodeJobs.Add(new VideoTranscodeJob { MediaId = media.Id, CreatedAt = _clock.UtcNow });
        }
        else
        {
            job.AttemptCount = 0;
            job.LeaseId = null;
            job.LeaseUntil = null;
            job.LastError = null;
            job.CreatedAt = _clock.UtcNow;
        }
        media.VideoStatus = VideoStatus.Queued;
        media.FailureCode = null;
        media.ProgressPercent = 0;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "LessonVideoRetried", _currentUser.UserId, "LessonMedia", media.Id,
            cancellationToken: cancellationToken);
    }
}

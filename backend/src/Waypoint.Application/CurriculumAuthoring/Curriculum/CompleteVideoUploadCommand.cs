using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;
using Waypoint.Domain.CurriculumAuthoring;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record CompleteVideoUploadCommand(Guid LessonId, Guid UploadId)
    : IRequest<LessonMediaResult>;

public sealed class CompleteVideoUploadCommandHandler
    : IRequestHandler<CompleteVideoUploadCommand, LessonMediaResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IUploadStore _uploads;
    private readonly IVideoTranscoder _transcoder;

    public CompleteVideoUploadCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IUploadStore uploads,
        IVideoTranscoder transcoder)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _uploads = uploads;
        _transcoder = transcoder;
    }

    public async Task<LessonMediaResult> Handle(
        CompleteVideoUploadCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.VideoUploadSessions.FirstOrDefaultAsync(
            item => item.Id == request.UploadId && item.LessonId == request.LessonId,
            cancellationToken);
        if (session is null || session.ExpiresAt <= _clock.UtcNow)
        {
            throw new NotFoundException("Upload not found.");
        }

        var missing = Enumerable.Range(0, session.ChunkCount)
            .Count(index => (session.ReceivedChunks[index / 8] & (1 << (index % 8))) == 0);
        if (missing > 0)
        {
            throw new ValidationException($"{missing} chunk(s) have not been received yet.");
        }

        if (_uploads.Length(session.Id) != session.ExpectedBytes)
        {
            throw new ValidationException("The uploaded file is not the expected size.");
        }

        var head = await _uploads.ReadHeadAsync(session.Id, 12, cancellationToken);
        if (!LessonVideoUploadPolicy.HasMp4Signature(head))
        {
            throw new ValidationException("The file content does not match a valid MP4 file.");
        }

        var probe = await _transcoder.ProbeAsync(_uploads.PartPath(session.Id), cancellationToken);
        if (probe is null)
        {
            throw new ValidationException("This file is not a video we can read.");
        }

        var media = new LessonMedia
        {
            LessonId = session.LessonId,
            MediaType = "video",
            OriginalFileName = session.FileName,
            ContentType = "video/mp4",
            SizeBytes = session.ExpectedBytes,
            Url = string.Empty,
            VideoStatus = VideoStatus.Queued,
            DurationSeconds = probe.DurationSeconds,
            Width = probe.Width,
            Height = probe.Height,
            CreatedAt = _clock.UtcNow,
        };
        _uploads.PromoteToSource(session.Id, media.Id);
        _db.LessonMedia.Add(media);
        _db.VideoTranscodeJobs.Add(new VideoTranscodeJob { MediaId = media.Id, CreatedAt = _clock.UtcNow });
        _db.VideoUploadSessions.Remove(session);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "LessonVideoUploaded",
            _currentUser.UserId,
            "LessonMedia",
            media.Id,
            cancellationToken: cancellationToken);
        return new LessonMediaResult(
            media.Id, media.MediaType, media.OriginalFileName, media.ContentType, media.SizeBytes,
            media.Url);
    }
}

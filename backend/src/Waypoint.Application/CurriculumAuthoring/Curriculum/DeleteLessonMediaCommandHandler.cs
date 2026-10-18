using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed class DeleteLessonMediaCommandHandler : IRequestHandler<DeleteLessonMediaCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaStore _store;
    private readonly IAfterCommitDispatcher _afterCommit;

    public DeleteLessonMediaCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IMediaStore store,
        IAfterCommitDispatcher afterCommit)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
        _store = store;
        _afterCommit = afterCommit;
    }

    public async Task Handle(DeleteLessonMediaCommand request, CancellationToken cancellationToken)
    {
        var media = await _db.LessonMedia.FirstOrDefaultAsync(
            item => item.Id == request.MediaId && item.LessonId == request.LessonId,
            cancellationToken) ?? throw new NotFoundException("Lesson media not found.");
        _db.LessonMedia.Remove(media);
        var job = await _db.VideoTranscodeJobs.FirstOrDefaultAsync(
            item => item.MediaId == media.Id, cancellationToken);
        if (job is not null) _db.VideoTranscodeJobs.Remove(job);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "LessonMediaRemoved",
            _currentUser.UserId,
            "LessonMedia",
            media.Id,
            cancellationToken: cancellationToken);

        if (media.VideoStatus != Domain.CurriculumAuthoring.VideoStatus.None)
        {
            // Revoke playback and free disk only once the delete has committed.
            await _afterCommit.DispatchBestEffortAsync(
                _ =>
                {
                    _store.MoveToTrash(media.Id);
                    return Task.CompletedTask;
                },
                cancellationToken);
        }
    }
}

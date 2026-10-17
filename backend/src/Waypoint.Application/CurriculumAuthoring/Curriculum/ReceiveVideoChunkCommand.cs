using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

/// <summary>
/// Carries the request body as a stream so a multi-megabyte chunk is never buffered in memory.
/// </summary>
public sealed record ReceiveVideoChunkCommand(
    Guid LessonId, Guid UploadId, int Index, long? ContentLength, Stream Body) : IRequest;

public sealed class ReceiveVideoChunkCommandHandler : IRequestHandler<ReceiveVideoChunkCommand>
{
    private const int MaxAttempts = 5;

    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IUploadStore _uploads;

    public ReceiveVideoChunkCommandHandler(IAppDbContext db, IClock clock, IUploadStore uploads)
    {
        _db = db;
        _clock = clock;
        _uploads = uploads;
    }

    public async Task Handle(ReceiveVideoChunkCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.VideoUploadSessions.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == request.UploadId && item.LessonId == request.LessonId,
            cancellationToken);
        if (session is null || session.ExpiresAt <= _clock.UtcNow)
        {
            throw new NotFoundException("Upload not found.");
        }

        if (request.Index < 0 || request.Index >= session.ChunkCount)
        {
            throw new ValidationException("The chunk index is outside the upload.");
        }

        var expected = request.Index < session.ChunkCount - 1
            ? session.ChunkSizeBytes
            : session.ExpectedBytes - (long)(session.ChunkCount - 1) * session.ChunkSizeBytes;
        if (request.ContentLength != expected)
        {
            throw new ValidationException($"The chunk must be exactly {expected} bytes.");
        }

        // The write is idempotent: a retried chunk overwrites identical bytes at the same offset.
        await _uploads.WriteChunkAsync(
            session.Id, (long)request.Index * session.ChunkSizeBytes, request.Body, expected,
            cancellationToken);

        await MarkReceivedAsync(session.Id, request.Index, cancellationToken);
    }

    // Several chunks are in flight at once, so the shared bit field is updated optimistically.
    private async Task MarkReceivedAsync(Guid uploadId, int index, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var tracked = await _db.VideoUploadSessions.FirstAsync(
                item => item.Id == uploadId, cancellationToken);
            tracked.ReceivedChunks = (byte[])tracked.ReceivedChunks.Clone();
            tracked.ReceivedChunks[index / 8] |= (byte)(1 << (index % 8));
            tracked.ConcurrencyStamp = Guid.NewGuid();
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < MaxAttempts)
            {
                // Another chunk request changed the bits first: reload the row and set our bit again.
                foreach (var entry in ex.Entries) await entry.ReloadAsync(cancellationToken);
            }
        }
    }
}

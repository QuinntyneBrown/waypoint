using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Domain.CurriculumAuthoring;

namespace Waypoint.Infrastructure;

/// <summary>
/// Claims queued lesson videos with an expiring lease, transcodes each once, and publishes the
/// result atomically. The lease is extended while ffmpeg runs, so a crashed host is detected when
/// the lease lapses and another instance picks the job up.
/// </summary>
public sealed class VideoTranscodeProcessor
{
    public const int MaxAttempts = 2;
    private const int BatchSize = 10;
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan MinimumTimeout = TimeSpan.FromMinutes(30);

    private readonly AppDbContext _db;
    private readonly IVideoTranscoder _transcoder;
    private readonly IMediaStore _store;
    private readonly IClock _clock;
    private readonly IServiceScopeFactory _scopes;
    private readonly MediaOptions _options;

    public VideoTranscodeProcessor(
        AppDbContext db,
        IVideoTranscoder transcoder,
        IMediaStore store,
        IClock clock,
        IServiceScopeFactory scopes,
        IOptions<MediaOptions> options)
    {
        _db = db;
        _transcoder = transcoder;
        _store = store;
        _clock = clock;
        _scopes = scopes;
        _options = options.Value;
    }

    /// <summary>Transcodes each available queued video once; returns how many were attempted.</summary>
    public Task<int> RunOnceAsync(CancellationToken cancellationToken = default) =>
        RunOnceAsync(null, cancellationToken);

    /// <summary>As above, limited to the videos of one lesson when a lesson is given.</summary>
    public async Task<int> RunOnceAsync(Guid? lessonId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return 0;
        var now = _clock.UtcNow;
        var ids = await _db.VideoTranscodeJobs.AsNoTracking()
            .Join(_db.LessonMedia, job => job.MediaId, media => media.Id, (job, media) => new { job, media })
            .Where(row => (lessonId == null || row.media.LessonId == lessonId)
                && (row.media.VideoStatus == VideoStatus.Queued
                    || row.media.VideoStatus == VideoStatus.Processing)
                && row.job.AttemptCount < MaxAttempts
                && (row.job.LeaseUntil == null || row.job.LeaseUntil <= now))
            .OrderBy(row => row.job.CreatedAt)
            .Select(row => row.job.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var id in ids)
        {
            var leaseId = Guid.NewGuid();
            if (!await TryClaimAsync(id, leaseId, _clock.UtcNow, cancellationToken)) continue;
            await ProcessAsync(id, leaseId, cancellationToken);
            processed++;
        }
        return processed;
    }

    /// <summary>Removes leftovers: trash, stale partial uploads, orphaned and abandoned folders.</summary>
    public async Task SweepAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var existing = (await _db.LessonMedia.AsNoTracking().Select(item => item.Id)
            .ToListAsync(cancellationToken)).ToHashSet();
        var transcoding = (await _db.VideoTranscodeJobs.AsNoTracking()
            .Where(job => job.LeaseUntil != null && job.LeaseUntil > now)
            .Select(job => job.MediaId).ToListAsync(cancellationToken)).ToHashSet();
        _store.Sweep(existing, transcoding, now - _options.UploadSessionLifetime);

        var expired = await _db.VideoUploadSessions
            .Where(session => session.ExpiresAt <= now).ToListAsync(cancellationToken);
        _db.VideoUploadSessions.RemoveRange(expired);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> TryClaimAsync(
        Guid jobId, Guid leaseId, DateTime now, CancellationToken cancellationToken)
    {
        if (_db.Database.IsRelational())
        {
            var count = await _db.VideoTranscodeJobs
                .Where(job => job.Id == jobId
                    && job.AttemptCount < MaxAttempts
                    && (job.LeaseUntil == null || job.LeaseUntil <= now))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(job => job.LeaseId, leaseId)
                    .SetProperty(job => job.LeaseUntil, now.Add(LeaseDuration)), cancellationToken);
            return count == 1;
        }

        var row = await _db.VideoTranscodeJobs.FirstOrDefaultAsync(job => job.Id == jobId, cancellationToken);
        if (row is null || row.AttemptCount >= MaxAttempts
            || row.LeaseUntil is DateTime leaseUntil && leaseUntil > now)
            return false;
        row.LeaseId = leaseId;
        row.LeaseUntil = now.Add(LeaseDuration);
        await _db.SaveChangesAsync(cancellationToken);
        _db.Entry(row).State = EntityState.Detached;
        return true;
    }

    private async Task ProcessAsync(Guid jobId, Guid leaseId, CancellationToken stoppingToken)
    {
        var job = await _db.VideoTranscodeJobs.AsTracking().FirstAsync(item => item.Id == jobId, stoppingToken);
        var media = await _db.LessonMedia.AsTracking().FirstAsync(item => item.Id == job.MediaId, stoppingToken);
        media.VideoStatus = VideoStatus.Processing;
        media.ProgressPercent = 0;
        job.StartedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(stoppingToken);

        if (!_store.SourceExists(media.Id))
        {
            await FailAsync(job, media, "SourceMissing", "The original video file is missing.", final: true);
            return;
        }

        var duration = TimeSpan.FromSeconds(media.DurationSeconds ?? 0);
        var timeout = TimeSpan.FromTicks(Math.Max(MinimumTimeout.Ticks, duration.Ticks * 3));
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        limit.CancelAfter(timeout);
        var percent = 0;
        var progress = new Progress<double>(value => percent = (int)Math.Clamp(value * 100, 0, 99));
        using var heartbeatStop = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var heartbeat = HeartbeatAsync(jobId, leaseId, () => percent, heartbeatStop.Token);
        try
        {
            var workDirectory = _store.PrepareWorkDirectory(media.Id);
            await _transcoder.TranscodeAsync(_store.SourcePath(media.Id), workDirectory, progress, limit.Token);
            await StopHeartbeatAsync(heartbeatStop, heartbeat);
            _store.Publish(media.Id);
            media.VideoStatus = VideoStatus.Ready;
            media.ProgressPercent = 100;
            media.FailureCode = null;
            _db.VideoTranscodeJobs.Remove(job);
            await _db.SaveChangesAsync(CancellationToken.None);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown: give the job back without counting an attempt.
            await StopHeartbeatAsync(heartbeatStop, heartbeat);
            _store.DiscardWork(media.Id);
            job.LeaseId = null;
            job.LeaseUntil = null;
            media.VideoStatus = VideoStatus.Queued;
            media.ProgressPercent = 0;
            await _db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            await StopHeartbeatAsync(heartbeatStop, heartbeat);
            var timedOut = exception is OperationCanceledException;
            await FailAsync(
                job, media,
                timedOut ? "TranscodeTimeout" : "TranscodeFailed",
                timedOut ? "The transcode exceeded its time limit." : exception.Message,
                final: false);
        }
    }

    private async Task FailAsync(
        VideoTranscodeJob job, LessonMedia media, string code, string message, bool final)
    {
        _store.DiscardWork(media.Id);
        job.AttemptCount = final ? MaxAttempts : job.AttemptCount + 1;
        job.LastError = message.Length > 500 ? message[..500] : message;
        job.LeaseId = null;
        job.LeaseUntil = null;
        media.ProgressPercent = 0;
        if (job.AttemptCount >= MaxAttempts)
        {
            media.VideoStatus = VideoStatus.Failed;
            media.FailureCode = code;
        }
        else
        {
            media.VideoStatus = VideoStatus.Queued;
        }
        await _db.SaveChangesAsync(CancellationToken.None);
    }

    private static async Task StopHeartbeatAsync(CancellationTokenSource stop, Task heartbeat)
    {
        await stop.CancelAsync();
        try { await heartbeat; } catch (OperationCanceledException) { }
    }

    // Extends the lease and records progress from a separate scope, because the processor's own
    // context is not safe to use from another thread while the transcode runs.
    private async Task HeartbeatAsync(
        Guid jobId, Guid leaseId, Func<int> percent, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(HeartbeatInterval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var job = await db.VideoTranscodeJobs.FirstOrDefaultAsync(
                item => item.Id == jobId && item.LeaseId == leaseId, cancellationToken);
            if (job is null) return;
            var now = _clock.UtcNow;
            job.LeaseUntil = now.Add(LeaseDuration);
            job.LastProgressAt = now;
            var media = await db.LessonMedia.FirstAsync(item => item.Id == job.MediaId, cancellationToken);
            media.ProgressPercent = percent();
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}

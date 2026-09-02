using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Engagement.Notifications;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

/// <summary>
/// Claims due reminder rows and publishes their selected channels independently of feed reads.
/// A stable scheduled-notification id deduplicates both the in-app row and outbound-email envelope.
/// </summary>
public sealed class ScheduledNotificationProcessor
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(10);
    private readonly AppDbContext _db;
    private readonly NotificationDeliveryService _delivery;
    private readonly IClock _clock;
    private readonly AfterCommitDispatcher _afterCommit;

    public ScheduledNotificationProcessor(
        AppDbContext db,
        NotificationDeliveryService delivery,
        IClock clock,
        AfterCommitDispatcher afterCommit)
    {
        _db = db;
        _delivery = delivery;
        _clock = clock;
        _afterCommit = afterCommit;
    }

    public async Task<int> RunOnceAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var ids = await _db.ScheduledNotifications.AsNoTracking()
            .Where(item => item.DeliveredAt == null
                && item.CancelledAt == null
                && (item.NextAttemptAt ?? item.DeliverAt) <= now
                && (item.LeaseUntil == null || item.LeaseUntil <= now))
            .OrderBy(item => item.NextAttemptAt ?? item.DeliverAt)
            .Select(item => item.Id)
            .Take(50)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var id in ids)
        {
            var leaseId = Guid.NewGuid();
            var claimNow = _clock.UtcNow;
            if (!await TryClaimAsync(id, leaseId, claimNow, cancellationToken)) continue;
            await DeliverClaimedAsync(id, leaseId, cancellationToken);
            processed++;
        }
        return processed;
    }

    private async Task<bool> TryClaimAsync(
        Guid id,
        Guid leaseId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (_db.Database.IsRelational())
        {
            var count = await _db.ScheduledNotifications
                .Where(item => item.Id == id
                    && item.DeliveredAt == null
                    && item.CancelledAt == null
                    && (item.NextAttemptAt ?? item.DeliverAt) <= now
                    && (item.LeaseUntil == null || item.LeaseUntil <= now))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.LeaseId, leaseId)
                    .SetProperty(item => item.LeaseUntil, now.Add(LeaseDuration)), cancellationToken);
            if (count == 1)
            {
                var stale = _db.ChangeTracker.Entries<ScheduledNotification>()
                    .FirstOrDefault(entry => entry.Entity.Id == id);
                if (stale is not null) stale.State = EntityState.Detached;
            }
            return count == 1;
        }

        var item = await _db.ScheduledNotifications.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null || item.DeliveredAt is not null || item.CancelledAt is not null
            || (item.NextAttemptAt ?? item.DeliverAt) > now
            || item.LeaseUntil is DateTime leaseUntil && leaseUntil > now)
            return false;
        item.LeaseId = leaseId;
        item.LeaseUntil = now.Add(LeaseDuration);
        await _db.SaveChangesAsync(cancellationToken);
        _db.Entry(item).State = EntityState.Detached;
        return true;
    }

    private async Task DeliverClaimedAsync(
        Guid id,
        Guid leaseId,
        CancellationToken cancellationToken)
    {
        if (!_db.Database.IsRelational())
        {
            await DeliverWithoutOwnedTransactionAsync(id, leaseId, cancellationToken);
            return;
        }

        if (_db.Database.CurrentTransaction is not null)
        {
            if (!_afterCommit.IsDeferring)
                throw new InvalidOperationException(
                    "Scheduled delivery inside an existing transaction requires a managed post-commit scope.");
            await CompleteDeliveryAsync(id, leaseId, cancellationToken);
            return;
        }

        var attemptedAt = _clock.UtcNow;
        Exception? failure = null;
        var cancelled = false;
        using (_afterCommit.Defer())
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await CompleteDeliveryAsync(id, leaseId, cancellationToken, attemptedAt);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _afterCommit.Discard();
                await transaction.RollbackAsync(CancellationToken.None);
                cancelled = true;
            }
            catch (Exception exception)
            {
                _afterCommit.Discard();
                await transaction.RollbackAsync(CancellationToken.None);
                failure = exception;
            }
        }

        if (!cancelled && failure is null)
        {
            await _afterCommit.FlushAsync(CancellationToken.None);
        }

        if (cancelled)
        {
            _db.ChangeTracker.Clear();
            await ReleaseLeaseAsync(id, leaseId);
            cancellationToken.ThrowIfCancellationRequested();
        }
        if (failure is not null)
        {
            _db.ChangeTracker.Clear();
            await RecordFailureAsync(id, leaseId, attemptedAt, failure, cancellationToken);
        }
    }

    private async Task DeliverWithoutOwnedTransactionAsync(
        Guid id,
        Guid leaseId,
        CancellationToken cancellationToken)
    {
        var item = await _db.ScheduledNotifications.FirstAsync(
            row => row.Id == id && row.LeaseId == leaseId,
            cancellationToken);
        var attemptedAt = _clock.UtcNow;
        try
        {
            await PrepareAndDispatchAsync(item, attemptedAt, cancellationToken);
            MarkDelivered(item, attemptedAt);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ClearLease(item);
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            MarkFailed(item, attemptedAt, exception);
        }

        ClearLease(item);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task CompleteDeliveryAsync(
        Guid id,
        Guid leaseId,
        CancellationToken cancellationToken,
        DateTime? attemptedAtOverride = null)
    {
        var item = await _db.ScheduledNotifications.FirstAsync(
            row => row.Id == id && row.LeaseId == leaseId,
            cancellationToken);
        var attemptedAt = attemptedAtOverride ?? _clock.UtcNow;
        await PrepareAndDispatchAsync(item, attemptedAt, cancellationToken);
        MarkDelivered(item, attemptedAt);
        ClearLease(item);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task PrepareAndDispatchAsync(
        ScheduledNotification item,
        DateTime attemptedAt,
        CancellationToken cancellationToken)
    {
        var plan = await _delivery.PrepareAsync(
            item.UserId,
            item.EventKey,
            item.Title,
            item.Body,
            item.LinkPath,
            attemptedAt,
            cancellationToken,
            item.Id);
        await _db.SaveChangesAsync(cancellationToken);
        await _delivery.DispatchAsync(plan, cancellationToken);
    }

    private async Task RecordFailureAsync(
        Guid id,
        Guid leaseId,
        DateTime attemptedAt,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var item = await _db.ScheduledNotifications.FirstOrDefaultAsync(
            row => row.Id == id && row.LeaseId == leaseId,
            cancellationToken);
        if (item is null) return;
        MarkFailed(item, attemptedAt, exception);
        ClearLease(item);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ReleaseLeaseAsync(Guid id, Guid leaseId)
    {
        await _db.ScheduledNotifications
            .Where(item => item.Id == id && item.LeaseId == leaseId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.LeaseId, (Guid?)null)
                .SetProperty(item => item.LeaseUntil, (DateTime?)null), CancellationToken.None);
    }

    private void MarkDelivered(ScheduledNotification item, DateTime attemptedAt)
    {
        item.DeliveredAt = _clock.UtcNow;
        item.LastAttemptAt = attemptedAt;
        item.AttemptCount++;
        item.NextAttemptAt = null;
        item.LastErrorCode = null;
    }

    private static void MarkFailed(
        ScheduledNotification item,
        DateTime attemptedAt,
        Exception exception)
    {
        item.LastAttemptAt = attemptedAt;
        item.AttemptCount++;
        item.LastErrorCode = SafeErrorCode(exception);
        var exponent = Math.Min(item.AttemptCount - 1, 8);
        item.NextAttemptAt = attemptedAt.AddMinutes(Math.Min(360, Math.Pow(2, exponent)));
    }

    private static void ClearLease(ScheduledNotification item)
    {
        item.LeaseId = null;
        item.LeaseUntil = null;
    }

    private static string SafeErrorCode(Exception exception)
    {
        var name = exception.GetType().Name;
        return name[..Math.Min(128, name.Length)];
    }
}

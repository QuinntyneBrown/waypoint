using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.ExceptionServices;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public interface IOutboundEmailProcessorRunner
{
    Task<int> RunOnceAsync(CancellationToken cancellationToken = default);
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default);
}

/// <summary>Claims and drains one bounded outbox batch. SQL claims are atomic across instances.</summary>
public sealed class OutboundEmailProcessor : IOutboundEmailProcessorRunner
{
    private readonly AppDbContext _db;
    private readonly ISmtpEmailTransport _transport;
    private readonly IClock _clock;
    private readonly SmtpOptions _options;
    private readonly IUserPrivacyLock _privacyLock;

    public OutboundEmailProcessor(
        AppDbContext db,
        ISmtpEmailTransport transport,
        IClock clock,
        IOptions<SmtpOptions> options,
        IUserPrivacyLock privacyLock)
    {
        _db = db;
        _transport = transport;
        _clock = clock;
        _options = options.Value;
        _privacyLock = privacyLock;
    }

    public async Task<int> RunOnceAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return 0;
        var now = _clock.UtcNow;
        var ids = await _db.OutboundEmails.AsNoTracking()
            .Where(item => item.SentAt == null
                && item.AttemptCount < _options.MaxAttempts
                && item.AvailableAt <= now
                && (item.LeaseUntil == null || item.LeaseUntil <= now))
            .OrderBy(item => item.CreatedAt)
            .Select(item => item.Id)
            .Take(25)
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

    /// <summary>
    /// Removes delivered envelopes and exhausted dead letters after the configured operations
    /// window. Live, retryable envelopes are never removed by retention cleanup.
    /// </summary>
    public async Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = _clock.UtcNow.AddDays(-_options.OutboxRetentionDays);
        if (_db.Database.IsRelational())
        {
            return await _db.OutboundEmails
                .Where(item => (item.SentAt != null && item.SentAt <= cutoff)
                    || (item.SentAt == null
                        && item.AttemptCount >= _options.MaxAttempts
                        && item.LastAttemptAt != null
                        && item.LastAttemptAt <= cutoff))
                .ExecuteDeleteAsync(cancellationToken);
        }

        var expired = await _db.OutboundEmails
            .Where(item => (item.SentAt != null && item.SentAt <= cutoff)
                || (item.SentAt == null
                    && item.AttemptCount >= _options.MaxAttempts
                    && item.LastAttemptAt != null
                    && item.LastAttemptAt <= cutoff))
            .ToListAsync(cancellationToken);
        if (expired.Count == 0) return 0;
        _db.OutboundEmails.RemoveRange(expired);
        return await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> TryClaimAsync(
        Guid id,
        Guid leaseId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (_db.Database.IsRelational())
        {
            var count = await _db.OutboundEmails
                .Where(item => item.Id == id
                    && item.SentAt == null
                    && item.AttemptCount < _options.MaxAttempts
                    && item.AvailableAt <= now
                    && (item.LeaseUntil == null || item.LeaseUntil <= now))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.LeaseId, leaseId)
                    .SetProperty(item => item.LeaseUntil, now.Add(LeaseDuration)), cancellationToken);
            if (count == 1)
            {
                var stale = _db.ChangeTracker.Entries<OutboundEmail>()
                    .FirstOrDefault(entry => entry.Entity.Id == id);
                if (stale is not null) stale.State = EntityState.Detached;
            }
            return count == 1;
        }

        var item = await _db.OutboundEmails.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null || item.SentAt is not null
            || item.AttemptCount >= _options.MaxAttempts || item.AvailableAt > now
            || item.LeaseUntil is DateTime leaseUntil && leaseUntil > now)
            return false;
        item.LeaseId = leaseId;
        item.LeaseUntil = now.Add(LeaseDuration);
        await _db.SaveChangesAsync(cancellationToken);
        _db.Entry(item).State = EntityState.Detached;
        return true;
    }

    // SMTP has a bounded end-to-end timeout. The safety margin also covers best-effort disconnect
    // and persistence, preventing another worker from reclaiming an envelope still being sent.
    private TimeSpan LeaseDuration => TimeSpan.FromSeconds(_options.TimeoutSeconds + 60);

    private async Task DeliverClaimedAsync(
        Guid id,
        Guid leaseId,
        CancellationToken cancellationToken)
    {
        var item = await _db.OutboundEmails.FirstAsync(
            row => row.Id == id && row.LeaseId == leaseId,
            cancellationToken);
        var linkedUserIds = new[] { item.RecipientUserId, item.SubjectUserId }
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Distinct()
            .OrderBy(value => value)
            .ToList();
        await using var privacyTransaction = linkedUserIds.Count > 0 && _db.Database.IsSqlServer()
            ? await _db.Database.BeginTransactionAsync(cancellationToken)
            : null;
        var attemptedAt = _clock.UtcNow;
        var removeAfterAttempt = false;
        ExceptionDispatchInfo? cancellation = null;
        try
        {
            foreach (var userId in linkedUserIds)
            {
                await _privacyLock.AcquireAsync(userId, cancellationToken);
            }

            // A DSR transaction that acquired the same lock first may have purged this claimed row.
            if (privacyTransaction is not null
                && !await _db.OutboundEmails.AsNoTracking().AnyAsync(
                    row => row.Id == id && row.LeaseId == leaseId,
                    cancellationToken))
            {
                _db.Entry(item).State = EntityState.Detached;
            }
            else if (await SuppressOrRefreshRecipientAsync(item, linkedUserIds, cancellationToken))
            {
                removeAfterAttempt = true;
            }
            else
            {
                await _transport.SendAsync(item, cancellationToken);
                item.SentAt = _clock.UtcNow;
                item.LastAttemptAt = attemptedAt;
                item.AttemptCount++;
                item.LastErrorCode = null;
                removeAfterAttempt = item.PurgeAfterDelivery;
            }
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            cancellation = ExceptionDispatchInfo.Capture(exception);
        }
        catch (Exception exception)
        {
            item.LastAttemptAt = attemptedAt;
            item.AttemptCount++;
            item.LastErrorCode = exception.GetType().Name[..Math.Min(128, exception.GetType().Name.Length)];
            var exponent = Math.Min(item.AttemptCount - 1, 8);
            item.AvailableAt = attemptedAt.AddMinutes(Math.Min(360, Math.Pow(2, exponent)));
            removeAfterAttempt = item.PurgeAfterDelivery
                && item.AttemptCount >= _options.MaxAttempts;
        }
        finally
        {
            if (_db.Entry(item).State == EntityState.Detached)
            {
                // The privacy transaction already erased the envelope.
            }
            else if (removeAfterAttempt)
            {
                _db.OutboundEmails.Remove(item);
            }
            else
            {
                item.LeaseId = null;
                item.LeaseUntil = null;
            }
            // Persist the SMTP outcome and release the lease even during graceful host shutdown.
            await _db.SaveChangesAsync(CancellationToken.None);
            if (privacyTransaction is not null)
            {
                await privacyTransaction.CommitAsync(CancellationToken.None);
            }
        }
        cancellation?.Throw();
    }

    private async Task<bool> SuppressOrRefreshRecipientAsync(
        OutboundEmail item,
        IReadOnlyList<Guid> linkedUserIds,
        CancellationToken cancellationToken)
    {
        if (linkedUserIds.Count == 0) return false;
        var users = await _db.Users.AsNoTracking()
            .Where(user => linkedUserIds.Contains(user.Id))
            .Select(user => new LinkedUserState(
                user.Id,
                user.Email,
                user.Status,
                user.PersonalDataAnonymizedAt))
            .ToListAsync(cancellationToken);

        if (item.RecipientUserId is Guid recipientUserId)
        {
            var recipient = users.SingleOrDefault(user => user.Id == recipientUserId);
            if (recipient is null || recipient.Status != UserStatus.Active)
            {
                return true;
            }
            item.ToEmail = recipient.Email;
        }

        if (item.SubjectUserId is Guid subjectUserId)
        {
            var subject = users.SingleOrDefault(user => user.Id == subjectUserId);
            if (subject is null)
            {
                return true;
            }
            var removed = subject.Status == UserStatus.Removed
                || subject.PersonalDataAnonymizedAt is not null;
            if (item.IsPrivacyRemovalConfirmation)
            {
                // This exception is valid only after the linked subject has actually been removed.
                return !removed;
            }
            if (removed)
            {
                return true;
            }
        }

        return false;
    }

    private sealed record LinkedUserState(
        Guid Id,
        string Email,
        UserStatus Status,
        DateTime? PersonalDataAnonymizedAt);
}

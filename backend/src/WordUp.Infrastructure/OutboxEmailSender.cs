using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

/// <summary>Persists email instead of performing network I/O in an HTTP/application transaction.</summary>
public sealed class OutboxEmailSender : IEmailSender
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IUserPrivacyLock _privacyLock;

    public OutboxEmailSender(
        IAppDbContext db,
        IClock clock,
        IUserPrivacyLock privacyLock)
    {
        _db = db;
        _clock = clock;
        _privacyLock = privacyLock;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default,
        string? idempotencyKey = null,
        Guid? subjectUserId = null,
        bool purgeAfterDelivery = false,
        Guid? recipientUserId = null,
        bool isPrivacyRemovalConfirmation = false)
    {
        if (isPrivacyRemovalConfirmation && subjectUserId is null)
        {
            throw new ArgumentException(
                "A privacy-removal confirmation must retain its data-subject link.",
                nameof(subjectUserId));
        }

        var linkedUserIds = new[] { recipientUserId, subjectUserId }
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .Distinct()
            .OrderBy(item => item)
            .ToList();
        foreach (var userId in linkedUserIds)
        {
            await _privacyLock.AcquireAsync(userId, cancellationToken);
        }

        var linkedUsers = linkedUserIds.Count == 0
            ? []
            : await _db.Users.AsNoTracking()
                .Where(item => linkedUserIds.Contains(item.Id))
                .Select(item => new LinkedUserState(
                    item.Id,
                    item.Email,
                    item.Status,
                    item.PersonalDataAnonymizedAt))
                .ToListAsync(cancellationToken);
        if (subjectUserId is Guid linkedSubjectId)
        {
            var subjectState = linkedUsers.SingleOrDefault(item => item.Id == linkedSubjectId);
            var subjectWasRemoved = subjectState is null
                || subjectState.Status == UserStatus.Removed
                || subjectState.PersonalDataAnonymizedAt is not null;
            if (subjectWasRemoved && !isPrivacyRemovalConfirmation)
            {
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey)
            && await _db.OutboundEmails.AsNoTracking().AnyAsync(
                item => item.IdempotencyKey == idempotencyKey,
                cancellationToken))
            return;
        if (recipientUserId is Guid linkedRecipientId)
        {
            var recipient = linkedUsers.SingleOrDefault(item => item.Id == linkedRecipientId);
            if (recipient is null || recipient.Status != UserStatus.Active)
            {
                return;
            }
            toEmail = recipient.Email;
        }

        var now = _clock.UtcNow;
        _db.OutboundEmails.Add(new OutboundEmail
        {
            ToEmail = toEmail.Trim().ToLowerInvariant(),
            Subject = subject.Trim(),
            Body = body,
            RecipientUserId = recipientUserId,
            SubjectUserId = subjectUserId,
            IsPrivacyRemovalConfirmation = isPrivacyRemovalConfirmation,
            PurgeAfterDelivery = purgeAfterDelivery,
            CreatedAt = now,
            AvailableAt = now,
            IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey,
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private sealed record LinkedUserState(
        Guid Id,
        string Email,
        UserStatus Status,
        DateTime? PersonalDataAnonymizedAt);
}

using System.Data.Common;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Waypoint.Application.Engagement.Notifications;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Completion;

[TestFixture]
[Property("Req", "L2-NFR-005")]
public sealed class AtomicOutboxTransactionTests
{
    [Test]
    public async Task Atomic_request_rolls_back_business_and_outbox_saves_together()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var clock = new MutableClock(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var sender = new OutboxEmailSender(db, clock, PrivacyLock(db));
        var afterCommit = CreateAfterCommit();
        var behavior = new AtomicOutboxBehavior<StubAtomicRequest, Unit>(db, afterCommit);
        var requestId = Guid.NewGuid();
        var idempotencyKey = $"atomic-test:{requestId:N}";

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await behavior.Handle(
                new StubAtomicRequest(),
                async () =>
                {
                    db.ContactRequests.Add(new ContactRequest
                    {
                        Id = requestId,
                        Name = "Atomic Test",
                        Email = "atomic@example.com",
                        Topic = "Atomicity",
                        Message = "This write must roll back.",
                        CreatedAt = clock.UtcNow,
                    });
                    await db.SaveChangesAsync();
                    await sender.SendAsync(
                        "atomic@example.com",
                        "Atomic test",
                        "This envelope must roll back.",
                        idempotencyKey: idempotencyKey);
                    throw new InvalidOperationException("fail after both saves");
                },
                CancellationToken.None));

        db.ChangeTracker.Clear();
        var businessExists = await db.ContactRequests.AnyAsync(item => item.Id == requestId);
        var outboxExists = await db.OutboundEmails.AnyAsync(
            item => item.IdempotencyKey == idempotencyKey);
        Assert.Multiple(() =>
        {
            Assert.That(businessExists, Is.False);
            Assert.That(outboxExists, Is.False);
        });
    }

    [Test]
    public async Task Commit_failure_discards_realtime_callback_without_a_phantom_event()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var commitFailure = new ThrowingCommitInterceptor();
        await using var db = CreateDb(connection, commitFailure);
        await db.Database.EnsureCreatedAsync();
        var clock = new MutableClock(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var user = await SeedRecipientAsync(db, clock, "commit-failure");
        var publisher = new StubChangePublisher(throwOnPublish: false);
        var afterCommit = CreateAfterCommit();
        var delivery = new NotificationDeliveryService(
            db,
            new OutboxEmailSender(db, clock, PrivacyLock(db)),
            publisher,
            new StubLinks(),
            afterCommit,
            PrivacyLock(db));
        var behavior = new AtomicOutboxBehavior<StubAtomicRequest, Unit>(db, afterCommit);
        commitFailure.Enabled = true;

        Assert.ThrowsAsync<CommitFailureException>(async () =>
            await behavior.Handle(
                new StubAtomicRequest(),
                async () =>
                {
                    var plan = await delivery.PrepareAsync(
                        user.Id,
                        "commit-failure",
                        "Commit failure",
                        "This event must never be published.",
                        "/test",
                        clock.UtcNow,
                        CancellationToken.None);
                    await db.SaveChangesAsync();
                    await delivery.DispatchAsync(plan, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None));

        commitFailure.Enabled = false;
        db.ChangeTracker.Clear();
        Assert.Multiple(() =>
        {
            Assert.That(publisher.UserEvents, Is.Zero);
            Assert.That(db.Notifications.Any(item => item.UserId == user.Id), Is.False);
            Assert.That(db.OutboundEmails.Count(), Is.Zero);
        });
    }

    [Test]
    public async Task Hub_failure_after_commit_does_not_roll_back_atomic_business_or_outbox()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var clock = new MutableClock(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var user = await SeedRecipientAsync(db, clock, "hub-failure");
        var publisher = new StubChangePublisher(throwOnPublish: true);
        var afterCommit = CreateAfterCommit();
        var delivery = new NotificationDeliveryService(
            db,
            new OutboxEmailSender(db, clock, PrivacyLock(db)),
            publisher,
            new StubLinks(),
            afterCommit,
            PrivacyLock(db));
        var behavior = new AtomicOutboxBehavior<StubAtomicRequest, Unit>(db, afterCommit);

        await behavior.Handle(
            new StubAtomicRequest(),
            async () =>
            {
                var plan = await delivery.PrepareAsync(
                    user.Id,
                    "hub-failure",
                    "Hub failure",
                    "Durable state must still commit.",
                    "/test",
                    clock.UtcNow,
                    CancellationToken.None);
                await db.SaveChangesAsync();
                await delivery.DispatchAsync(plan, CancellationToken.None);
                return Unit.Value;
            },
            CancellationToken.None);

        db.ChangeTracker.Clear();
        Assert.Multiple(() =>
        {
            Assert.That(publisher.UserEvents, Is.EqualTo(1));
            Assert.That(db.Notifications.Count(item => item.UserId == user.Id), Is.EqualTo(1));
            Assert.That(db.OutboundEmails.Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Scheduled_delivery_rolls_back_notification_and_outbox_before_recording_retry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var clock = new MutableClock(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var user = new User
        {
            Email = $"scheduled-atomic-{Guid.NewGuid():N}@example.com",
            DisplayName = "Scheduled atomic recipient",
            PasswordHash = "hash",
            Role = Roles.Youth,
            Status = UserStatus.Active,
        };
        var scheduled = new ScheduledNotification
        {
            UserId = user.Id,
            EventKey = "session-reminders",
            Title = "Session soon",
            Body = "Your session starts soon.",
            LinkPath = "/youth/schedule",
            SourceType = "AtomicTest",
            SourceId = Guid.NewGuid(),
            DeliverAt = clock.UtcNow.AddMinutes(-1),
            CreatedAt = clock.UtcNow.AddDays(-1),
        };
        db.AddRange(
            user,
            scheduled,
            new NotificationPreference
            {
                UserId = user.Id,
                EventKey = scheduled.EventKey,
                InApp = true,
                Email = true,
            });
        await db.SaveChangesAsync();

        var sender = new OutboxEmailSender(db, clock, PrivacyLock(db));
        var failingSender = new EnqueueThenThrowEmailSender(sender);
        var afterCommit = CreateAfterCommit();
        var failingDelivery = new NotificationDeliveryService(
            db,
            failingSender,
            new StubChangePublisher(throwOnPublish: false),
            new StubLinks(),
            afterCommit,
            PrivacyLock(db));
        var processor = new ScheduledNotificationProcessor(db, failingDelivery, clock, afterCommit);

        Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));
        db.ChangeTracker.Clear();
        var failed = await db.ScheduledNotifications.SingleAsync(item => item.Id == scheduled.Id);
        Assert.Multiple(() =>
        {
            Assert.That(failed.DeliveredAt, Is.Null);
            Assert.That(failed.AttemptCount, Is.EqualTo(1));
            Assert.That(failed.NextAttemptAt, Is.Not.Null);
            Assert.That(failed.LeaseId, Is.Null);
            Assert.That(failed.LeaseUntil, Is.Null);
            Assert.That(db.Notifications.Any(item => item.ScheduledNotificationId == scheduled.Id), Is.False);
            Assert.That(
                db.OutboundEmails.Any(item => item.IdempotencyKey == $"scheduled-notification:{scheduled.Id:N}"),
                Is.False);
        });

        clock.UtcNow = failed.NextAttemptAt!.Value;
        var successfulDelivery = new NotificationDeliveryService(
            db,
            sender,
            new StubChangePublisher(throwOnPublish: false),
            new StubLinks(),
            afterCommit,
            PrivacyLock(db));
        processor = new ScheduledNotificationProcessor(db, successfulDelivery, clock, afterCommit);
        Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));

        db.ChangeTracker.Clear();
        var delivered = await db.ScheduledNotifications.SingleAsync(item => item.Id == scheduled.Id);
        Assert.Multiple(() =>
        {
            Assert.That(delivered.DeliveredAt, Is.Not.Null);
            Assert.That(delivered.AttemptCount, Is.EqualTo(2));
            Assert.That(delivered.LeaseId, Is.Null);
            Assert.That(delivered.LeaseUntil, Is.Null);
            Assert.That(db.Notifications.Count(item => item.ScheduledNotificationId == scheduled.Id), Is.EqualTo(1));
            Assert.That(
                db.OutboundEmails.Count(item => item.IdempotencyKey == $"scheduled-notification:{scheduled.Id:N}"),
                Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Scheduled_hub_failure_occurs_after_durable_delivery_commit()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var clock = new MutableClock(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var user = await SeedRecipientAsync(db, clock, "scheduled-hub-failure");
        var scheduled = new ScheduledNotification
        {
            UserId = user.Id,
            EventKey = "scheduled-hub-failure",
            Title = "Scheduled hub failure",
            Body = "Durable state must still commit.",
            SourceType = "AtomicTest",
            SourceId = Guid.NewGuid(),
            DeliverAt = clock.UtcNow.AddMinutes(-1),
            CreatedAt = clock.UtcNow.AddDays(-1),
        };
        db.ScheduledNotifications.Add(scheduled);
        await db.SaveChangesAsync();

        var publisher = new StubChangePublisher(throwOnPublish: true);
        var afterCommit = CreateAfterCommit();
        var delivery = new NotificationDeliveryService(
            db,
            new OutboxEmailSender(db, clock, PrivacyLock(db)),
            publisher,
            new StubLinks(),
            afterCommit,
            PrivacyLock(db));
        var processor = new ScheduledNotificationProcessor(db, delivery, clock, afterCommit);

        Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));
        db.ChangeTracker.Clear();
        var delivered = await db.ScheduledNotifications.SingleAsync(item => item.Id == scheduled.Id);
        Assert.Multiple(() =>
        {
            Assert.That(publisher.UserEvents, Is.EqualTo(1));
            Assert.That(delivered.DeliveredAt, Is.Not.Null);
            Assert.That(delivered.AttemptCount, Is.EqualTo(1));
            Assert.That(delivered.NextAttemptAt, Is.Null);
            Assert.That(db.Notifications.Count(item => item.ScheduledNotificationId == scheduled.Id), Is.EqualTo(1));
            Assert.That(
                db.OutboundEmails.Count(item => item.IdempotencyKey == $"scheduled-notification:{scheduled.Id:N}"),
                Is.EqualTo(1));
        });
    }

    private sealed record StubAtomicRequest : IRequest<Unit>, IAtomicRequest;

    private static AppDbContext CreateDb(
        SqliteConnection connection,
        params IInterceptor[] interceptors)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection);
        if (interceptors.Length > 0) optionsBuilder.AddInterceptors(interceptors);
        return new AppDbContext(
            optionsBuilder.Options,
            new AesSensitiveDataProtector(Options.Create(new SensitiveDataOptions
            {
                EncryptionKey = "atomic-outbox-test-encryption-key-32-characters",
            })));
    }

    private static AfterCommitDispatcher CreateAfterCommit() =>
        new(NullLogger<AfterCommitDispatcher>.Instance);

    private static IUserPrivacyLock PrivacyLock(AppDbContext db) =>
        new SqlServerUserPrivacyLock(db, Options.Create(new SmtpOptions()));

    private static async Task<User> SeedRecipientAsync(
        AppDbContext db,
        IClock clock,
        string eventKey)
    {
        var user = new User
        {
            Email = $"{eventKey}-{Guid.NewGuid():N}@example.com",
            DisplayName = "Realtime recipient",
            PasswordHash = "hash",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CreatedAt = clock.UtcNow,
        };
        db.AddRange(
            user,
            new NotificationPreference
            {
                UserId = user.Id,
                EventKey = eventKey,
                InApp = true,
                Email = true,
            });
        await db.SaveChangesAsync();
        return user;
    }

    private sealed class MutableClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow { get; set; } = utcNow;
    }

    private sealed class StubLinks : IApplicationLinkBuilder
    {
        public string Portal(string relativePath) => $"https://portal.waypoint.test/{relativePath.TrimStart('/')}";
        public string PublicSite(string relativePath) => $"https://waypoint.test/{relativePath.TrimStart('/')}";
    }

    private sealed class StubChangePublisher(bool throwOnPublish) : IChangePublisher
    {
        public int UserEvents { get; private set; }

        public Task PublishToUserAsync(
            Guid userId,
            string eventName,
            object payload,
            CancellationToken cancellationToken = default)
        {
            UserEvents++;
            return throwOnPublish
                ? throw new PublishFailureException()
                : Task.CompletedTask;
        }

        public Task PublishToRoleAsync(
            string role,
            string eventName,
            object payload,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task PublishToCohortAsync(
            Guid cohortId,
            string eventName,
            object payload,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class EnqueueThenThrowEmailSender(IEmailSender inner) : IEmailSender
    {
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
            await inner.SendAsync(
                toEmail,
                subject,
                body,
                cancellationToken,
                idempotencyKey,
                subjectUserId,
                purgeAfterDelivery,
                recipientUserId,
                isPrivacyRemovalConfirmation);
            throw new EmailEnqueueFailureException();
        }
    }

    private sealed class ThrowingCommitInterceptor : DbTransactionInterceptor
    {
        public bool Enabled { get; set; }

        public override ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default) => Enabled
                ? ValueTask.FromException<InterceptionResult>(new CommitFailureException())
                : ValueTask.FromResult(result);
    }

    private sealed class CommitFailureException : Exception { }
    private sealed class EmailEnqueueFailureException : Exception { }
    private sealed class PublishFailureException : Exception { }
}

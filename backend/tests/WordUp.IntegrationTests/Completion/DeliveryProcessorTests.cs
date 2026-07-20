using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Mentoring.Flags;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Completion;

// Traces to: L2-ENGAGE-008, L2-NFR-003, L2-NFR-005, L2-NFR-011
[TestFixture]
[Property("Req", "L2-ENGAGE-008")]
public sealed class DeliveryProcessorTests
{
    [Test]
    public async Task Encrypted_outbox_retries_and_deduplicates_by_stable_key()
    {
        await using var db = CreateDb();
        var clock = new MutableClock(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var sender = new OutboxEmailSender(db, clock, PrivacyLock(db));
        await sender.SendAsync("person@example.com", "Subject", "Private body", idempotencyKey: "stable-key");
        await sender.SendAsync("person@example.com", "Subject", "Private body", idempotencyKey: "stable-key");
        Assert.That(await db.OutboundEmails.CountAsync(), Is.EqualTo(1));
        Assert.That(
            db.Model.FindEntityType(typeof(OutboundEmail))!
                .FindProperty(nameof(OutboundEmail.Body))!
                .GetValueConverter(),
            Is.Not.Null,
            "outbox body must use the authenticated-encryption converter");

        var transport = new StubSmtpTransport { Fail = true };
        var processor = new OutboundEmailProcessor(
            db,
            transport,
            clock,
            Options.Create(new SmtpOptions { Enabled = true }),
            PrivacyLock(db));
        Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));
        var queued = await db.OutboundEmails.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(queued.AttemptCount, Is.EqualTo(1));
            Assert.That(queued.SentAt, Is.Null);
            Assert.That(queued.LastErrorCode, Is.EqualTo(nameof(StubDeliveryException)));
            Assert.That(queued.AvailableAt, Is.GreaterThan(clock.UtcNow));
        });

        clock.UtcNow = queued.AvailableAt;
        transport.Fail = false;
        Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));
        db.ChangeTracker.Clear();
        queued = await db.OutboundEmails.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(queued.AttemptCount, Is.EqualTo(2));
            Assert.That(queued.SentAt, Is.Not.Null);
            Assert.That(transport.Attempts, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Sent_and_dead_envelopes_expire_and_max_attempts_stop_delivery()
    {
        await using var db = CreateDb();
        var now = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        var clock = new MutableClock(now);
        db.OutboundEmails.AddRange(
            Envelope("old-sent@example.com", now.AddDays(-31), sentAt: now.AddDays(-31)),
            Envelope("old-dead@example.com", now.AddDays(-40), attempts: 2, lastAttemptAt: now.AddDays(-31)),
            Envelope("recent-sent@example.com", now.AddDays(-5), sentAt: now.AddDays(-5)),
            Envelope("retry@example.com", now.AddDays(-1), attempts: 1, lastAttemptAt: now.AddDays(-1)));
        await db.SaveChangesAsync();

        var transport = new StubSmtpTransport { Fail = true };
        var processor = new OutboundEmailProcessor(
            db,
            transport,
            clock,
            Options.Create(new SmtpOptions
            {
                Enabled = true,
                MaxAttempts = 2,
                OutboxRetentionDays = 30,
            }),
            PrivacyLock(db));

        Assert.That(await processor.PurgeExpiredAsync(), Is.EqualTo(2));
        Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));
        Assert.That(await processor.RunOnceAsync(), Is.Zero, "an exhausted envelope must not be claimed again");
        Assert.That(transport.Attempts, Is.EqualTo(1));

        clock.UtcNow = now.AddDays(31);
        Assert.That(await processor.PurgeExpiredAsync(), Is.EqualTo(2));
        Assert.That(await db.OutboundEmails.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Each_claim_uses_a_fresh_clock_and_a_lease_longer_than_the_smtp_timeout()
    {
        await using var db = CreateDb();
        var batchReadAt = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        var claimAt = batchReadAt.AddMinutes(5);
        var clock = new SequenceClock(batchReadAt, claimAt, claimAt, claimAt);
        db.OutboundEmails.Add(Envelope("lease@example.com", batchReadAt.AddMinutes(-1)));
        await db.SaveChangesAsync();
        var transport = new StubSmtpTransport();
        var processor = new OutboundEmailProcessor(
            db,
            transport,
            clock,
            Options.Create(new SmtpOptions
            {
                Enabled = true,
                TimeoutSeconds = 300,
            }),
            PrivacyLock(db));

        Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));
        Assert.That(transport.ObservedLeaseUntil, Is.EqualTo(claimAt.AddSeconds(360)));
    }

    [Test]
    public async Task Accepted_smtp_message_is_successful_even_when_quit_is_rejected()
    {
        await using var server = new RejectingQuitSmtpServer();
        var received = server.AcceptOneAsync();
        var transport = new SmtpEmailTransport(Options.Create(new SmtpOptions
        {
            Enabled = true,
            Host = "127.0.0.1",
            Port = server.Port,
            UseSsl = false,
            UseStartTls = false,
            FromEmail = "noreply@wordup.test",
            FromName = "Word Up",
            TimeoutSeconds = 5,
        }));

        await transport.SendAsync(
            Envelope("recipient@example.com", DateTime.UtcNow),
            CancellationToken.None);

        Assert.That(await received, Is.True, "the SMTP server accepted the DATA before rejecting QUIT");
    }

    [Test]
    public async Task Cleanup_failure_does_not_starve_outbound_delivery()
    {
        var runner = new CleanupFailingRunner();
        var services = new ServiceCollection()
            .AddSingleton<IOutboundEmailProcessorRunner>(runner)
            .BuildServiceProvider();
        var worker = new OutboundEmailWorker(
            services.GetRequiredService<IServiceScopeFactory>(),
            new StubHostEnvironment(),
            NullLogger<OutboundEmailWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await runner.DeliveryRan.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await worker.StopAsync(CancellationToken.None);
        await services.DisposeAsync();

        Assert.Multiple(() =>
        {
            Assert.That(runner.CleanupAttempts, Is.EqualTo(1));
            Assert.That(runner.DeliveryAttempts, Is.EqualTo(1));
        });
    }

    [Test]
    public void Every_email_producing_mediator_workflow_is_marked_atomic()
    {
        var producerDependencies = new[]
        {
            typeof(IEmailSender),
            typeof(NotificationDeliveryService),
            typeof(AtRiskEvaluator),
        };
        var unmarked = typeof(WordUp.Application.ApplicationDependencyInjection).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract)
            .Where(type => type.GetConstructors().Any(constructor =>
                constructor.GetParameters().Any(parameter =>
                    producerDependencies.Contains(parameter.ParameterType))))
            .SelectMany(type => type.GetInterfaces()
                .Where(contract => contract.IsGenericType
                    && contract.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>))
                .Select(contract => new
                {
                    Handler = type.Name,
                    Request = contract.GetGenericArguments()[0],
                })
                .Concat(type.GetInterfaces()
                    .Where(contract => contract.IsGenericType
                        && contract.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<>))
                    .Select(contract => new
                    {
                        Handler = type.Name,
                        Request = contract.GetGenericArguments()[0],
                    })))
            .Where(item => !typeof(IAtomicRequest).IsAssignableFrom(item.Request))
            .Select(item => $"{item.Handler} ({item.Request.Name})")
            .Order()
            .ToList();

        Assert.That(unmarked, Is.Empty,
            $"Email-producing requests must implement {nameof(IAtomicRequest)}: {string.Join(", ", unmarked)}");
    }

    [Test]
    public void Every_realtime_publisher_consumer_uses_the_post_commit_dispatcher()
    {
        var unsafeConsumers = typeof(WordUp.Application.ApplicationDependencyInjection).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract)
            .Where(type => type.GetConstructors().Any(constructor =>
                constructor.GetParameters().Any(parameter =>
                    parameter.ParameterType == typeof(IChangePublisher))))
            .Where(type => type.GetConstructors().All(constructor =>
                constructor.GetParameters().All(parameter =>
                    parameter.ParameterType != typeof(IAfterCommitDispatcher))))
            .Select(type => type.Name)
            .Order()
            .ToList();

        Assert.That(unsafeConsumers, Is.Empty,
            $"Realtime publishers must dispatch after commit: {string.Join(", ", unsafeConsumers)}");
    }

    [Test]
    public async Task Anonymization_removes_linked_envelopes_without_claiming_unlinked_shared_email()
    {
        await using var db = CreateDb();
        var now = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        var user = new User
        {
            Email = "remove-me@example.com",
            DisplayName = "Remove Me",
            PasswordHash = "hash",
            Role = Roles.Youth,
            Status = UserStatus.Active,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var sender = new OutboxEmailSender(db, new MutableClock(now), PrivacyLock(db));
        await sender.SendAsync(
            user.Email,
            "Account directed",
            "Private body one",
            recipientUserId: user.Id);
        await sender.SendAsync(
            "guardian.external@example.com",
            "Subject linked",
            "Private body two",
            subjectUserId: user.Id);
        await sender.SendAsync(
            user.Email.ToUpperInvariant(),
            "Unlinked shared address",
            "Do not infer ownership from this address");
        await sender.SendAsync("someone-else@example.com", "Other", "Keep this envelope");

        var anonymizer = new WordUp.Application.PlatformServices.Privacy.PersonalDataAnonymizer(db);
        await anonymizer.AnonymizeAsync(user, now, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Multiple(() =>
        {
            Assert.That(user.Email, Does.EndWith("@anonymized.invalid"));
            Assert.That(db.OutboundEmails.Count(), Is.EqualTo(2));
            Assert.That(
                db.OutboundEmails.Select(item => item.ToEmail),
                Is.EquivalentTo(new[]
                {
                    "remove-me@example.com",
                    "someone-else@example.com",
                }));
        });
    }

    [Test]
    public async Task Scheduled_worker_dispatches_once_without_a_notification_feed_read()
    {
        await using var db = CreateDb();
        var clock = new MutableClock(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var user = new User
        {
            Email = "scheduled@example.com",
            DisplayName = "Scheduled recipient",
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
            SourceType = "Session",
            SourceId = Guid.NewGuid(),
            DeliverAt = clock.UtcNow.AddMinutes(-1),
            CreatedAt = clock.UtcNow.AddDays(-1),
        };
        db.AddRange(
            user,
            new NotificationPreference
            {
                UserId = user.Id,
                EventKey = scheduled.EventKey,
                InApp = true,
                Email = true,
            },
            scheduled);
        await db.SaveChangesAsync();

        var emails = new CapturingEmailSender();
        var changes = new StubChangePublisher();
        var afterCommit = new AfterCommitDispatcher(NullLogger<AfterCommitDispatcher>.Instance);
        var delivery = new NotificationDeliveryService(
            db,
            emails,
            changes,
            new StubLinks(),
            afterCommit,
            PrivacyLock(db));
        var processor = new ScheduledNotificationProcessor(db, delivery, clock, afterCommit);
        Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));
        db.ChangeTracker.Clear();
        var delivered = await db.ScheduledNotifications.SingleAsync(item => item.Id == scheduled.Id);
        Assert.Multiple(() =>
        {
            Assert.That(delivered.DeliveredAt, Is.Not.Null);
            Assert.That(delivered.AttemptCount, Is.EqualTo(1));
            Assert.That(db.Notifications.Single().ScheduledNotificationId, Is.EqualTo(scheduled.Id));
            Assert.That(emails.AllTo(user.Email).Count, Is.EqualTo(1));
            Assert.That(changes.UserEvents, Is.EqualTo(1));
        });

        Assert.That(await processor.RunOnceAsync(), Is.Zero);
        Assert.That(emails.AllTo(user.Email).Count, Is.EqualTo(1));

        var second = new ScheduledNotification
        {
            UserId = user.Id,
            EventKey = "session-reminders",
            Title = "Another reminder",
            Body = "This must wait for the worker.",
            SourceType = "Session",
            SourceId = Guid.NewGuid(),
            DeliverAt = clock.UtcNow.AddMinutes(-1),
        };
        db.ScheduledNotifications.Add(second);
        await db.SaveChangesAsync();
        var feed = new NotificationFeedQueryHandler(db, new StubCurrentUser(user.Id));
        await feed.Handle(new NotificationFeedQuery(), CancellationToken.None);
        Assert.That(second.DeliveredAt, Is.Null, "reading the feed must not dispatch scheduled work");
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"completion-delivery-{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(
            options,
            new AesSensitiveDataProtector(Options.Create(new SensitiveDataOptions
            {
                EncryptionKey = "completion-delivery-encryption-key-32-characters",
            })));
    }

    private static IUserPrivacyLock PrivacyLock(AppDbContext db) =>
        new SqlServerUserPrivacyLock(db, Options.Create(new SmtpOptions()));

    private static OutboundEmail Envelope(
        string to,
        DateTime createdAt,
        int attempts = 0,
        DateTime? lastAttemptAt = null,
        DateTime? sentAt = null) => new()
        {
            ToEmail = to,
            Subject = "Subject",
            Body = "Body",
            CreatedAt = createdAt,
            AvailableAt = createdAt,
            AttemptCount = attempts,
            LastAttemptAt = lastAttemptAt,
            SentAt = sentAt,
        };

    private sealed class MutableClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow { get; set; } = utcNow;
    }

    private sealed class SequenceClock(params DateTime[] values) : IClock
    {
        private readonly Queue<DateTime> _values = new(values);

        public DateTime UtcNow => _values.Dequeue();
    }

    private sealed class StubSmtpTransport : ISmtpEmailTransport
    {
        public bool Fail { get; set; }
        public int Attempts { get; private set; }
        public DateTime? ObservedLeaseUntil { get; private set; }

        public Task SendAsync(OutboundEmail message, CancellationToken cancellationToken)
        {
            Attempts++;
            ObservedLeaseUntil = message.LeaseUntil;
            if (Fail) throw new StubDeliveryException();
            return Task.CompletedTask;
        }
    }

    private sealed class StubDeliveryException : Exception { }

    private sealed class RejectingQuitSmtpServer : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);

        public RejectingQuitSmtpServer() => _listener.Start();

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        public async Task<bool> AcceptOneAsync()
        {
            using var client = await _listener.AcceptTcpClientAsync();
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            await using var writer = new StreamWriter(stream)
            {
                AutoFlush = true,
                NewLine = "\r\n",
            };
            await writer.WriteLineAsync("220 wordup.test ESMTP");
            var dataAccepted = false;
            while (await reader.ReadLineAsync() is { } command)
            {
                if (command.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase)
                    || command.StartsWith("HELO", StringComparison.OrdinalIgnoreCase)
                    || command.StartsWith("MAIL FROM", StringComparison.OrdinalIgnoreCase)
                    || command.StartsWith("RCPT TO", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("250 OK");
                }
                else if (command.Equals("DATA", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("354 End data with <CR><LF>.<CR><LF>");
                    while (await reader.ReadLineAsync() is { } dataLine && dataLine != ".") { }
                    dataAccepted = true;
                    await writer.WriteLineAsync("250 queued");
                }
                else if (command.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("500 disconnect deliberately rejected");
                    return dataAccepted;
                }
                else
                {
                    await writer.WriteLineAsync("250 OK");
                }
            }
            return dataAccepted;
        }

        public ValueTask DisposeAsync()
        {
            _listener.Stop();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CleanupFailingRunner : IOutboundEmailProcessorRunner
    {
        public int CleanupAttempts { get; private set; }
        public int DeliveryAttempts { get; private set; }
        public TaskCompletionSource<bool> DeliveryRan { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
        {
            CleanupAttempts++;
            throw new StubDeliveryException();
        }

        public Task<int> RunOnceAsync(CancellationToken cancellationToken = default)
        {
            DeliveryAttempts++;
            DeliveryRan.TrySetResult(true);
            return Task.FromResult(1);
        }
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "WordUp.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class StubLinks : IApplicationLinkBuilder
    {
        public string Portal(string relativePath) => $"https://portal.wordup.test/{relativePath.TrimStart('/')}";
        public string PublicSite(string relativePath) => $"https://wordup.test/{relativePath.TrimStart('/')}";
    }

    private sealed class StubCurrentUser(Guid userId) : ICurrentUser
    {
        public Guid? UserId => userId;
        public string? Role => Roles.Youth;
        public int? SessionVersion => null;
        public bool IsAuthenticated => true;
    }

    private sealed class StubChangePublisher : IChangePublisher
    {
        public int UserEvents { get; private set; }
        public Task PublishToUserAsync(
            Guid userId,
            string eventName,
            object payload,
            CancellationToken cancellationToken = default)
        {
            UserEvents++;
            return Task.CompletedTask;
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
}

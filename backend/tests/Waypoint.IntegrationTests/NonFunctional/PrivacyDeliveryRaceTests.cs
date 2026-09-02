using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Waypoint.Application.Engagement.Notifications;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-003")]
public sealed class PrivacyDeliveryRaceTests : IntegrationTestBase
{
    private static readonly DateTime Now =
        new(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task Claimed_email_waits_for_correction_commit_and_uses_authoritative_address()
    {
        var oldEmail = $"race-old-{Guid.NewGuid():N}@example.com";
        var newEmail = $"race-new-{Guid.NewGuid():N}@example.com";
        var (user, envelope) = await SeedEnvelopeAsync(oldEmail);

        await using var correctionScope = Factory.Services.CreateAsyncScope();
        var correctionDb = correctionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var correctionLock = correctionScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>();
        await using var correctionTransaction = await correctionDb.Database.BeginTransactionAsync();
        await correctionLock.AcquireAsync(user.Id);
        var correctedUser = await correctionDb.Users.SingleAsync(item => item.Id == user.Id);
        correctedUser.Email = newEmail;
        await correctionDb.SaveChangesAsync();

        await using var workerScope = Factory.Services.CreateAsyncScope();
        var workerDb = workerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var workerLock = new SignalingPrivacyLock(
            workerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
            attempted);
        var transport = new CapturingTransport();
        var processor = new OutboundEmailProcessor(
            workerDb,
            transport,
            new FixedClock(Now),
            EnabledSmtp(),
            workerLock);
        var processing = processor.RunOnceAsync();

        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Multiple(() =>
        {
            Assert.That(processing.IsCompleted, Is.False,
                "delivery must remain behind the correction transaction's privacy lock");
            Assert.That(transport.Attempts, Is.Zero);
        });

        await correctionTransaction.CommitAsync();
        Assert.That(await processing.WaitAsync(TimeSpan.FromSeconds(10)), Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(transport.Attempts, Is.EqualTo(1));
            Assert.That(transport.LastToEmail, Is.EqualTo(newEmail));
        });

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That((await verify.OutboundEmails.FindAsync(envelope.Id))!.SentAt, Is.Not.Null);
    }

    [Test]
    public async Task Claimed_email_waits_for_removal_commit_and_is_purged_without_transport()
    {
        var oldEmail = $"race-remove-{Guid.NewGuid():N}@example.com";
        var (user, envelope) = await SeedEnvelopeAsync(oldEmail);

        await using var removalScope = Factory.Services.CreateAsyncScope();
        var removalDb = removalScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var removalLock = removalScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>();
        await using var removalTransaction = await removalDb.Database.BeginTransactionAsync();
        await removalLock.AcquireAsync(user.Id);
        var removedUser = await removalDb.Users.SingleAsync(item => item.Id == user.Id);
        removedUser.Email = $"removed+{user.Id:N}@anonymized.invalid";
        removedUser.Status = UserStatus.Removed;
        removedUser.PersonalDataAnonymizedAt = Now;
        await removalDb.SaveChangesAsync();

        await using var workerScope = Factory.Services.CreateAsyncScope();
        var workerDb = workerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new CapturingTransport();
        var processor = new OutboundEmailProcessor(
            workerDb,
            transport,
            new FixedClock(Now),
            EnabledSmtp(),
            new SignalingPrivacyLock(
                workerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
                attempted));
        var processing = processor.RunOnceAsync();

        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(transport.Attempts, Is.Zero);
        Assert.That(processing.IsCompleted, Is.False);
        await removalTransaction.CommitAsync();

        Assert.That(await processing.WaitAsync(TimeSpan.FromSeconds(10)), Is.EqualTo(1));
        Assert.That(transport.Attempts, Is.Zero);
        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await verify.OutboundEmails.AnyAsync(item => item.Id == envelope.Id), Is.False);
    }

    [Test]
    public async Task Application_approval_email_readdresses_the_linked_guardian_after_correction()
    {
        var oldEmail = $"decision-guardian-old-{Guid.NewGuid():N}@example.com";
        var newEmail = $"decision-guardian-new-{Guid.NewGuid():N}@example.com";
        var guardian = await SeedUserAsync(oldEmail);
        var youth = await SeedUserAsync($"decision-youth-{Guid.NewGuid():N}@example.com");
        var envelope = await SeedEnvelopeAsync(
            oldEmail,
            "Your Waypoint application was approved",
            "Decision content about the linked youth.",
            guardian.Id,
            youth.Id);

        await using var correctionScope = Factory.Services.CreateAsyncScope();
        var correctionDb = correctionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var correctionTransaction = await correctionDb.Database.BeginTransactionAsync();
        await correctionScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>()
            .AcquireAsync(guardian.Id);
        (await correctionDb.Users.SingleAsync(item => item.Id == guardian.Id)).Email = newEmail;
        await correctionDb.SaveChangesAsync();

        await using var workerScope = Factory.Services.CreateAsyncScope();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new CapturingTransport();
        var processing = new OutboundEmailProcessor(
            workerScope.ServiceProvider.GetRequiredService<AppDbContext>(),
            transport,
            new FixedClock(Now),
            EnabledSmtp(),
            new SignalingPrivacyLock(
                workerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
                attempted))
            .RunOnceAsync();

        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(transport.Attempts, Is.Zero);
        await correctionTransaction.CommitAsync();
        Assert.That(await processing.WaitAsync(TimeSpan.FromSeconds(10)), Is.EqualTo(1));
        Assert.That(transport.LastToEmail, Is.EqualTo(newEmail));

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That((await verify.OutboundEmails.FindAsync(envelope.Id))!.SentAt, Is.Not.Null);
    }

    [Test]
    public async Task Application_approval_email_is_suppressed_when_its_youth_subject_is_removed()
    {
        var guardian = await SeedUserAsync(
            $"decision-guardian-{Guid.NewGuid():N}@example.com");
        var youth = await SeedUserAsync($"decision-remove-{Guid.NewGuid():N}@example.com");
        var envelope = await SeedEnvelopeAsync(
            guardian.Email,
            "Your Waypoint application was approved",
            "Decision content about the linked youth.",
            guardian.Id,
            youth.Id);

        await using var removalScope = Factory.Services.CreateAsyncScope();
        var removalDb = removalScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var removalTransaction = await removalDb.Database.BeginTransactionAsync();
        await removalScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>()
            .AcquireAsync(youth.Id);
        var removedYouth = await removalDb.Users.SingleAsync(item => item.Id == youth.Id);
        removedYouth.Status = UserStatus.Removed;
        removedYouth.PersonalDataAnonymizedAt = Now;
        await removalDb.SaveChangesAsync();

        await using var workerScope = Factory.Services.CreateAsyncScope();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new CapturingTransport();
        var processing = new OutboundEmailProcessor(
            workerScope.ServiceProvider.GetRequiredService<AppDbContext>(),
            transport,
            new FixedClock(Now),
            EnabledSmtp(),
            new SignalingPrivacyLock(
                workerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
                attempted))
            .RunOnceAsync();

        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(transport.Attempts, Is.Zero);
        await removalTransaction.CommitAsync();
        Assert.That(await processing.WaitAsync(TimeSpan.FromSeconds(10)), Is.EqualTo(1));
        Assert.That(transport.Attempts, Is.Zero);

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await verify.OutboundEmails.AnyAsync(item => item.Id == envelope.Id), Is.False);
    }

    [Test]
    public async Task Notification_producer_waits_for_removal_commit_and_creates_no_delivery_state()
    {
        var user = await SeedUserAsync($"notification-race-{Guid.NewGuid():N}@example.com");
        await using var removalScope = Factory.Services.CreateAsyncScope();
        var removalDb = removalScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var removalTransaction = await removalDb.Database.BeginTransactionAsync();
        await removalScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>()
            .AcquireAsync(user.Id);
        var removedUser = await removalDb.Users.SingleAsync(item => item.Id == user.Id);
        removedUser.Status = UserStatus.Removed;
        removedUser.Email = $"removed+{user.Id:N}@anonymized.invalid";
        removedUser.PersonalDataAnonymizedAt = Now;
        await removalDb.SaveChangesAsync();

        await using var producerScope = Factory.Services.CreateAsyncScope();
        var producerDb = producerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var signalingLock = new SignalingPrivacyLock(
            producerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
            attempted);
        var afterCommit = new AfterCommitDispatcher(NullLogger<AfterCommitDispatcher>.Instance);
        var delivery = new NotificationDeliveryService(
            producerDb,
            new OutboxEmailSender(producerDb, new FixedClock(Now), signalingLock),
            new NullChangePublisher(),
            new StubLinks(),
            afterCommit,
            signalingLock);

        var producing = ProduceNotificationAsync(producerDb, delivery, user.Id);
        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(producing.IsCompleted, Is.False,
            "notification state must remain behind the removal transaction's privacy lock");
        await removalTransaction.CommitAsync();
        await producing.WaitAsync(TimeSpan.FromSeconds(10));

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(verify.Notifications.Any(item => item.UserId == user.Id), Is.False);
            Assert.That(verify.OutboundEmails.Any(item => item.RecipientUserId == user.Id), Is.False);
        });
    }

    [Test]
    public async Task Subject_linked_notification_uses_global_lock_order_and_is_suppressed_after_removal()
    {
        Guid subjectId;
        Guid recipientId;
        do
        {
            subjectId = Guid.NewGuid();
            recipientId = Guid.NewGuid();
        }
        while (subjectId.CompareTo(recipientId) >= 0);

        var subject = await SeedUserAsync(
            $"notification-subject-{Guid.NewGuid():N}@example.com",
            subjectId);
        var recipient = await SeedUserAsync(
            $"notification-recipient-{Guid.NewGuid():N}@example.com",
            recipientId,
            emailNotifications: false);

        await using var removalScope = Factory.Services.CreateAsyncScope();
        var removalDb = removalScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var removalLock = removalScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>();
        await using var removalTransaction = await removalDb.Database.BeginTransactionAsync();
        await removalLock.AcquireAsync(subject.Id);
        var removedSubject = await removalDb.Users.SingleAsync(item => item.Id == subject.Id);
        removedSubject.Status = UserStatus.Removed;
        removedSubject.PersonalDataAnonymizedAt = Now;
        await removalDb.SaveChangesAsync();

        await using var producerScope = Factory.Services.CreateAsyncScope();
        var producerDb = producerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var firstAttempt = new TaskCompletionSource<Guid>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var signalingLock = new FirstAttemptPrivacyLock(
            producerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
            firstAttempt);
        var delivery = new NotificationDeliveryService(
            producerDb,
            new OutboxEmailSender(producerDb, new FixedClock(Now), signalingLock),
            new NullChangePublisher(),
            new StubLinks(),
            new AfterCommitDispatcher(NullLogger<AfterCommitDispatcher>.Instance),
            signalingLock);
        var producing = ProduceNotificationAsync(
            producerDb,
            delivery,
            recipient.Id,
            subject.Id);

        Assert.That(
            await firstAttempt.Task.WaitAsync(TimeSpan.FromSeconds(5)),
            Is.EqualTo(subject.Id),
            "the lower subject id must be locked before the recipient id");
        Assert.That(producing.IsCompleted, Is.False);

        // If the producer had locked recipient first, this acquisition would form the inverse
        // subject->recipient edge and deadlock with its recipient->subject edge.
        await removalLock.AcquireAsync(recipient.Id).WaitAsync(TimeSpan.FromSeconds(5));
        await removalTransaction.CommitAsync();
        await producing.WaitAsync(TimeSpan.FromSeconds(10));

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(verify.Notifications.Any(item => item.UserId == recipient.Id), Is.False);
            Assert.That(verify.OutboundEmails.Any(item => item.SubjectUserId == subject.Id), Is.False);
        });
    }

    [Test]
    public async Task Linked_outbox_enqueue_waits_for_removal_commit_and_is_suppressed()
    {
        var user = await SeedUserAsync($"enqueue-race-{Guid.NewGuid():N}@example.com");
        await using var removalScope = Factory.Services.CreateAsyncScope();
        var removalDb = removalScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var removalTransaction = await removalDb.Database.BeginTransactionAsync();
        await removalScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>()
            .AcquireAsync(user.Id);
        var removedUser = await removalDb.Users.SingleAsync(item => item.Id == user.Id);
        removedUser.Status = UserStatus.Removed;
        removedUser.PersonalDataAnonymizedAt = Now;
        await removalDb.SaveChangesAsync();

        await using var producerScope = Factory.Services.CreateAsyncScope();
        var producerDb = producerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new OutboxEmailSender(
            producerDb,
            new FixedClock(Now),
            new SignalingPrivacyLock(
                producerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
                attempted));
        var enqueue = EnqueueAsync(producerDb, sender, user);

        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(enqueue.IsCompleted, Is.False);
        await removalTransaction.CommitAsync();
        await enqueue.WaitAsync(TimeSpan.FromSeconds(10));

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await verify.OutboundEmails.AnyAsync(item => item.RecipientUserId == user.Id), Is.False);
    }

    [Test]
    public async Task Subject_only_external_enqueue_waits_for_removal_and_is_suppressed()
    {
        var youth = await SeedUserAsync($"subject-race-{Guid.NewGuid():N}@example.com");
        await using var removalScope = Factory.Services.CreateAsyncScope();
        var removalDb = removalScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var removalTransaction = await removalDb.Database.BeginTransactionAsync();
        await removalScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>()
            .AcquireAsync(youth.Id);
        var removedYouth = await removalDb.Users.SingleAsync(item => item.Id == youth.Id);
        removedYouth.Status = UserStatus.Removed;
        removedYouth.PersonalDataAnonymizedAt = Now;
        await removalDb.SaveChangesAsync();

        await using var producerScope = Factory.Services.CreateAsyncScope();
        var producerDb = producerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new OutboxEmailSender(
            producerDb,
            new FixedClock(Now),
            new SignalingPrivacyLock(
                producerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
                attempted));
        var enqueue = EnqueueSubjectOnlyAsync(producerDb, sender, youth.Id);

        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(enqueue.IsCompleted, Is.False);
        await removalTransaction.CommitAsync();
        await enqueue.WaitAsync(TimeSpan.FromSeconds(10));

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await verify.OutboundEmails.AnyAsync(item => item.SubjectUserId == youth.Id), Is.False);
    }

    [Test]
    public async Task Processor_retains_privacy_lock_until_smtp_and_outcome_commit_complete()
    {
        var (user, _) = await SeedEnvelopeAsync($"smtp-lock-{Guid.NewGuid():N}@example.com");
        await using var workerScope = Factory.Services.CreateAsyncScope();
        var workerDb = workerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transport = new BlockingTransport();
        var processor = new OutboundEmailProcessor(
            workerDb,
            transport,
            new FixedClock(Now),
            EnabledSmtp(),
            workerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>());
        var processing = processor.RunOnceAsync();
        await transport.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await using var correctionScope = Factory.Services.CreateAsyncScope();
        var correctionDb = correctionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var correctionTransaction = await correctionDb.Database.BeginTransactionAsync();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var correctionLock = new SignalingPrivacyLock(
            correctionScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>(),
            attempted);
        var acquiring = correctionLock.AcquireAsync(user.Id);
        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(acquiring.IsCompleted, Is.False,
            "a correction must remain blocked while SMTP is inside the worker's privacy lock");

        transport.Release.TrySetResult();
        Assert.That(await processing.WaitAsync(TimeSpan.FromSeconds(10)), Is.EqualTo(1));
        await acquiring.WaitAsync(TimeSpan.FromSeconds(10));
        await correctionTransaction.RollbackAsync();
    }

    [Test]
    public async Task Privacy_lock_timeout_is_a_retryable_domain_conflict()
    {
        var user = await SeedUserAsync($"lock-timeout-{Guid.NewGuid():N}@example.com");
        await using var ownerScope = Factory.Services.CreateAsyncScope();
        var ownerDb = ownerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var ownerTransaction = await ownerDb.Database.BeginTransactionAsync();
        await ownerScope.ServiceProvider.GetRequiredService<IUserPrivacyLock>().AcquireAsync(user.Id);

        await using var contenderScope = Factory.Services.CreateAsyncScope();
        var contenderDb = contenderScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var contenderTransaction = await contenderDb.Database.BeginTransactionAsync();
        var shortLock = new SqlServerUserPrivacyLock(
            contenderDb,
            EnabledSmtp(),
            TimeSpan.FromMilliseconds(100));

        var exception = Assert.ThrowsAsync<ConflictException>(async () =>
            await shortLock.AcquireAsync(user.Id));
        Assert.That(exception!.StatusCode, Is.EqualTo(409));
        await contenderTransaction.RollbackAsync();
        await ownerTransaction.RollbackAsync();
    }

    [Test]
    public async Task Concurrent_verified_ownership_claims_allow_one_subject_and_reject_the_other()
    {
        var firstSubject = await SeedUserAsync($"claim-first-{Guid.NewGuid():N}@example.com");
        var secondSubject = await SeedUserAsync($"claim-second-{Guid.NewGuid():N}@example.com");
        var contact = new ContactRequest
        {
            Name = "Anonymous author",
            Email = "shared-address@example.com",
            Topic = "Verified ownership",
            Message = "Record requiring separate identity verification.",
            CreatedAt = Now,
        };
        await using (var seedScope = Factory.Services.CreateAsyncScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            seedDb.ContactRequests.Add(contact);
            await seedDb.SaveChangesAsync();
        }

        await using var winnerScope = Factory.Services.CreateAsyncScope();
        var winnerDb = winnerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var winnerTransaction = await winnerDb.Database.BeginTransactionAsync();
        var winnerCount = await winnerScope.ServiceProvider
            .GetRequiredService<IVerifiedRecordOwnershipClaimer>()
            .ClaimAsync(firstSubject.Id, [contact.Id], [], [], [], []);
        Assert.That(winnerCount, Is.EqualTo(1));

        await using var contenderScope = Factory.Services.CreateAsyncScope();
        var contenderDb = contenderScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var contenderSessionId = await GetSessionIdAsync(contenderDb);
        await using var contenderTransaction = await contenderDb.Database.BeginTransactionAsync();
        var contenderClaim = contenderScope.ServiceProvider
            .GetRequiredService<IVerifiedRecordOwnershipClaimer>()
            .ClaimAsync(secondSubject.Id, [contact.Id], [], [], [], []);

        await using (var observerScope = Factory.Services.CreateAsyncScope())
        {
            var observerDb = observerScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await WaitUntilBlockedAsync(observerDb, contenderSessionId);
        }
        Assert.That(contenderClaim.IsCompleted, Is.False,
            "the competing conditional update must wait on the winning row lock");

        await winnerTransaction.CommitAsync();
        var conflict = Assert.ThrowsAsync<ConflictException>(async () =>
            await contenderClaim.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.That(conflict!.StatusCode, Is.EqualTo(409));
        await contenderTransaction.RollbackAsync();

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(
            (await verify.ContactRequests.AsNoTracking().SingleAsync(item => item.Id == contact.Id))
                .OwnerUserId,
            Is.EqualTo(firstSubject.Id));
    }

    private async Task<(User User, OutboundEmail Envelope)> SeedEnvelopeAsync(string email)
    {
        var user = await SeedUserAsync(email);
        var envelope = new OutboundEmail
        {
            ToEmail = email,
            Subject = "Concurrent delivery",
            Body = "Private body",
            RecipientUserId = user.Id,
            SubjectUserId = user.Id,
            CreatedAt = Now,
            AvailableAt = Now,
        };
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.OutboundEmails.Add(envelope);
        await db.SaveChangesAsync();
        return (user, envelope);
    }

    private async Task<OutboundEmail> SeedEnvelopeAsync(
        string email,
        string subject,
        string body,
        Guid? recipientUserId,
        Guid? subjectUserId)
    {
        var envelope = new OutboundEmail
        {
            ToEmail = email,
            Subject = subject,
            Body = body,
            RecipientUserId = recipientUserId,
            SubjectUserId = subjectUserId,
            CreatedAt = Now,
            AvailableAt = Now,
        };
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.OutboundEmails.Add(envelope);
        await db.SaveChangesAsync();
        return envelope;
    }

    private async Task<User> SeedUserAsync(
        string email,
        Guid? id = null,
        bool emailNotifications = true)
    {
        var user = new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            DisplayName = "Privacy race subject",
            PasswordHash = "hash",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CreatedAt = Now,
        };
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(user);
        db.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = user.Id,
            EventKey = "privacy-race",
            InApp = true,
            Email = emailNotifications,
        });
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task<int> GetSessionIdAsync(AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT @@SPID;";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task WaitUntilBlockedAsync(AppDbContext observerDb, int sessionId)
    {
        var connection = observerDb.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*)
                FROM sys.dm_exec_requests
                WHERE session_id = @sessionId
                  AND blocking_session_id <> 0;
                """;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@sessionId";
            parameter.Value = sessionId;
            command.Parameters.Add(parameter);
            if (Convert.ToInt32(await command.ExecuteScalarAsync()) > 0)
            {
                return;
            }
            await Task.Delay(25);
        }

        Assert.Fail("The competing verified-ownership update did not enter a blocked SQL state.");
    }

    private static async Task ProduceNotificationAsync(
        AppDbContext db,
        NotificationDeliveryService delivery,
        Guid userId,
        Guid? subjectUserId = null)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var plan = await delivery.PrepareAsync(
            userId,
            "privacy-race",
            "Private notification",
            "This must not survive removal.",
            "/privacy-race",
            Now,
            CancellationToken.None,
            subjectUserId: subjectUserId);
        await db.SaveChangesAsync();
        await delivery.DispatchAsync(plan, CancellationToken.None);
        await transaction.CommitAsync();
    }

    private static async Task EnqueueAsync(
        AppDbContext db,
        IEmailSender sender,
        User user)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        await sender.SendAsync(
            user.Email,
            "Private account email",
            "This must not survive removal.",
            recipientUserId: user.Id,
            subjectUserId: user.Id);
        await transaction.CommitAsync();
    }

    private static async Task EnqueueSubjectOnlyAsync(
        AppDbContext db,
        IEmailSender sender,
        Guid subjectUserId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        await sender.SendAsync(
            "guardian.external@example.com",
            "Guardian update",
            "Private information about the linked youth.",
            subjectUserId: subjectUserId);
        await transaction.CommitAsync();
    }

    private static IOptions<SmtpOptions> EnabledSmtp() =>
        Options.Create(new SmtpOptions
        {
            Enabled = true,
            TimeoutSeconds = 5,
        });

    private sealed class SignalingPrivacyLock(
        IUserPrivacyLock inner,
        TaskCompletionSource attempted) : IUserPrivacyLock
    {
        public async Task AcquireAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            attempted.TrySetResult();
            await inner.AcquireAsync(userId, cancellationToken);
        }
    }

    private sealed class FirstAttemptPrivacyLock(
        IUserPrivacyLock inner,
        TaskCompletionSource<Guid> firstAttempt) : IUserPrivacyLock
    {
        public async Task AcquireAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            firstAttempt.TrySetResult(userId);
            await inner.AcquireAsync(userId, cancellationToken);
        }
    }

    private sealed class CapturingTransport : ISmtpEmailTransport
    {
        public int Attempts { get; private set; }
        public string? LastToEmail { get; private set; }

        public Task SendAsync(OutboundEmail message, CancellationToken cancellationToken)
        {
            Attempts++;
            LastToEmail = message.ToEmail;
            return Task.CompletedTask;
        }
    }

    private sealed class BlockingTransport : ISmtpEmailTransport
    {
        public TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task SendAsync(OutboundEmail message, CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
        }
    }

    private sealed class FixedClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow => utcNow;
    }

    private sealed class StubLinks : IApplicationLinkBuilder
    {
        public string Portal(string relativePath) => $"https://portal.waypoint.test/{relativePath.TrimStart('/')}";
        public string PublicSite(string relativePath) => $"https://waypoint.test/{relativePath.TrimStart('/')}";
    }

    private sealed class NullChangePublisher : IChangePublisher
    {
        public Task PublishToUserAsync(
            Guid userId,
            string eventName,
            object payload,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

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

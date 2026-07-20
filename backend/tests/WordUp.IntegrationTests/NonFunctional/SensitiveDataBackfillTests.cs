using System.Data;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-003")]
public sealed class SensitiveDataBackfillTests : IntegrationTestBase
{
    [Test]
    public async Task Privacy_ephemeral_outbox_rows_are_erased_after_success_or_terminal_failure()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateRelationalDb(connection);
        await db.Database.EnsureCreatedAsync();
        Assert.That(db.Database.IsRelational(), Is.True,
            "this regression must exercise the SQL claim-and-deliver path");
        var clock = new MutableClock(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var options = Options.Create(new SmtpOptions
        {
            Enabled = true,
            MaxAttempts = 2,
        });
        var removedSubject = new User
        {
            Email = "removed-subject@anonymized.invalid",
            DisplayName = "Former participant",
            PasswordHash = "[removed]",
            Role = Roles.Youth,
            Status = UserStatus.Removed,
            PersonalDataAnonymizedAt = clock.UtcNow,
        };
        db.Users.Add(removedSubject);
        var delivered = new OutboundEmail
        {
            ToEmail = "privacy-delivered@example.com",
            Subject = "Removal completed",
            Body = "Privacy-sensitive completion envelope",
            SubjectUserId = removedSubject.Id,
            IsPrivacyRemovalConfirmation = true,
            PurgeAfterDelivery = true,
            CreatedAt = clock.UtcNow,
            AvailableAt = clock.UtcNow,
        };
        db.OutboundEmails.Add(delivered);
        await db.SaveChangesAsync();

        var successfulTransport = new StubSmtpTransport();
        var privacyLock = new SqlServerUserPrivacyLock(db, options);
        var successfulProcessor = new OutboundEmailProcessor(
            db,
            successfulTransport,
            clock,
            options,
            privacyLock);
        Assert.That(await successfulProcessor.RunOnceAsync(), Is.EqualTo(1));
        db.ChangeTracker.Clear();
        var deliveredEnvelopeRemains = await db.OutboundEmails.AnyAsync(item => item.Id == delivered.Id);
        Assert.Multiple(() =>
        {
            Assert.That(successfulTransport.Attempts, Is.EqualTo(1));
            Assert.That(deliveredEnvelopeRemains, Is.False);
        });

        var exhausted = new OutboundEmail
        {
            ToEmail = "privacy-failed@example.com",
            Subject = "Removal completed",
            Body = "Privacy-sensitive completion envelope",
            SubjectUserId = removedSubject.Id,
            IsPrivacyRemovalConfirmation = true,
            PurgeAfterDelivery = true,
            CreatedAt = clock.UtcNow,
            AvailableAt = clock.UtcNow,
        };
        db.OutboundEmails.Add(exhausted);
        await db.SaveChangesAsync();
        var failedTransport = new StubSmtpTransport { Fail = true };
        var failedProcessor = new OutboundEmailProcessor(db, failedTransport, clock, options, privacyLock);

        Assert.That(await failedProcessor.RunOnceAsync(), Is.EqualTo(1));
        db.ChangeTracker.Clear();
        var retryable = await db.OutboundEmails.SingleAsync(item => item.Id == exhausted.Id);
        Assert.Multiple(() =>
        {
            Assert.That(retryable.AttemptCount, Is.EqualTo(1));
            Assert.That(retryable.SentAt, Is.Null);
            Assert.That(retryable.PurgeAfterDelivery, Is.True);
        });

        clock.UtcNow = retryable.AvailableAt;
        Assert.That(await failedProcessor.RunOnceAsync(), Is.EqualTo(1));
        db.ChangeTracker.Clear();
        var exhaustedEnvelopeRemains = await db.OutboundEmails.AnyAsync(item => item.Id == exhausted.Id);
        Assert.Multiple(() =>
        {
            Assert.That(failedTransport.Attempts, Is.EqualTo(2));
            Assert.That(exhaustedEnvelopeRemains, Is.False,
                "a removal envelope must be erased once retry policy is exhausted");
        });
    }

    [Test]
    public void Every_scoped_personal_string_uses_randomized_authenticated_encryption()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        const string sentinel = "scoped personal sentinel";
        var properties = new (Type Entity, string Property, int MinimumStorage)[]
        {
            (typeof(AuditEntry), nameof(AuditEntry.Source), 2048),
            (typeof(AuditEntry), nameof(AuditEntry.Details), 12000),
            (typeof(Notification), nameof(Notification.Title), 2048),
            (typeof(Notification), nameof(Notification.Body), 12000),
            (typeof(ScheduledNotification), nameof(ScheduledNotification.Title), 2048),
            (typeof(ScheduledNotification), nameof(ScheduledNotification.Body), 12000),
            (typeof(QuizAttempt), nameof(QuizAttempt.AnswersJson), 0),
            (typeof(CodingLessonWorkspace), nameof(CodingLessonWorkspace.Code), 0),
            (typeof(CompetencyAssessment), nameof(CompetencyAssessment.Notes), 0),
            (typeof(Flag), nameof(Flag.Reason), 12000),
            (typeof(Flag), nameof(Flag.AutomaticReason), 12000),
            (typeof(Flag), nameof(Flag.ResolutionReason), 1024),
            (typeof(AssignedWork), nameof(AssignedWork.Message), 24000),
            (typeof(SessionRsvp), nameof(SessionRsvp.Note), 8000),
            (typeof(GuardianConsentRequest), nameof(GuardianConsentRequest.Destination), 2048),
            (typeof(StoredUserFile), nameof(StoredUserFile.OriginalFileName), 2048),
            (typeof(StoredSubmissionFile), nameof(StoredSubmissionFile.OriginalFileName), 2048),
            (typeof(YouthApplication), nameof(YouthApplication.FirstName), 1024),
            (typeof(YouthApplication), nameof(YouthApplication.LastName), 1024),
            (typeof(YouthApplication), nameof(YouthApplication.WhoIsFilling), 512),
            (typeof(YouthApplication), nameof(YouthApplication.Neighbourhood), 2048),
            (typeof(YouthApplication), nameof(YouthApplication.CohortInterest), 2048),
            (typeof(YouthApplication), nameof(YouthApplication.WhyInterested), 12000),
            (typeof(YouthApplication), nameof(YouthApplication.TrackInterest), 2048),
            (typeof(YouthApplication), nameof(YouthApplication.GuardianName), 2048),
            (typeof(YouthApplication), nameof(YouthApplication.GuardianEmail), 2048),
            (typeof(YouthApplication), nameof(YouthApplication.GuardianPhone), 1024),
            (typeof(YouthApplication), nameof(YouthApplication.DecisionNote), 12000),
            (typeof(ContactRequest), nameof(ContactRequest.Name), 1024),
            (typeof(ContactRequest), nameof(ContactRequest.Email), 2048),
            (typeof(ContactRequest), nameof(ContactRequest.Topic), 1024),
            (typeof(ContactRequest), nameof(ContactRequest.Message), 24000),
            (typeof(VolunteerSignup), nameof(VolunteerSignup.Name), 1024),
            (typeof(VolunteerSignup), nameof(VolunteerSignup.Email), 2048),
            (typeof(VolunteerSignup), nameof(VolunteerSignup.Phone), 1024),
            (typeof(VolunteerSignup), nameof(VolunteerSignup.AreasOfInterest), 4000),
            (typeof(DonationPledge), nameof(DonationPledge.DonorName), 1024),
            (typeof(DonationPledge), nameof(DonationPledge.Email), 2048),
            (typeof(DonationPledge), nameof(DonationPledge.ProviderReference), 2048),
        };

        foreach (var (entity, propertyName, minimumStorage) in properties)
        {
            var property = db.Model.FindEntityType(entity)!.FindProperty(propertyName)!;
            var converter = property.GetValueConverter();
            Assert.That(converter, Is.Not.Null, $"{entity.Name}.{propertyName}");
            var first = (string)converter!.ConvertToProvider(sentinel)!;
            var second = (string)converter.ConvertToProvider(sentinel)!;
            Assert.Multiple(() =>
            {
                Assert.That(first, Does.StartWith("enc:v1:"), $"{entity.Name}.{propertyName}");
                Assert.That(first, Is.Not.EqualTo(second), $"{entity.Name}.{propertyName} must use a random nonce");
                Assert.That(converter.ConvertFromProvider(first), Is.EqualTo(sentinel));
                if (minimumStorage > 0)
                    Assert.That(property.GetMaxLength(), Is.GreaterThanOrEqualTo(minimumStorage));
            });
        }

        var binarySentinel = "private file bytes"u8.ToArray();
        foreach (var (entity, propertyName) in new[]
        {
            (typeof(StoredUserFile), nameof(StoredUserFile.Content)),
            (typeof(StoredSubmissionFile), nameof(StoredSubmissionFile.Content)),
        })
        {
            var converter = db.Model.FindEntityType(entity)!.FindProperty(propertyName)!
                .GetValueConverter();
            Assert.That(converter, Is.Not.Null, $"{entity.Name}.{propertyName}");
            var first = (byte[])converter!.ConvertToProvider(binarySentinel)!;
            var second = (byte[])converter.ConvertToProvider(binarySentinel)!;
            Assert.Multiple(() =>
            {
                Assert.That(first.AsSpan().StartsWith("enc:b1:"u8), Is.True);
                Assert.That(first.SequenceEqual(second), Is.False,
                    $"{entity.Name}.{propertyName} must use a random nonce");
                Assert.That((byte[])converter.ConvertFromProvider(first)!, Is.EqualTo(binarySentinel));
            });
        }
    }

    [Test]
    public async Task Different_protection_keys_use_distinct_cached_models_and_round_trip_independently()
    {
        const string firstPlaintext = "first-key private value";
        const string secondPlaintext = "second-key private value";
        var firstProtector = CreateProtector("first-model-cache-protection-key-32-characters");
        var secondProtector = CreateProtector("second-model-cache-protection-key-32-characters");
        await using var firstConnection = new SqliteConnection("Data Source=:memory:");
        await using var secondConnection = new SqliteConnection("Data Source=:memory:");
        await firstConnection.OpenAsync();
        await secondConnection.OpenAsync();
        await using var firstDb = CreateRelationalDb(firstConnection, firstProtector);
        await using var secondDb = CreateRelationalDb(secondConnection, secondProtector);
        await firstDb.Database.EnsureCreatedAsync();
        await secondDb.Database.EnsureCreatedAsync();
        var firstContact = new ContactRequest
        {
            Name = "First key",
            Email = "first-key@example.com",
            Topic = "model cache",
            Message = firstPlaintext,
        };
        var secondContact = new ContactRequest
        {
            Name = "Second key",
            Email = "second-key@example.com",
            Topic = "model cache",
            Message = secondPlaintext,
        };
        firstDb.ContactRequests.Add(firstContact);
        secondDb.ContactRequests.Add(secondContact);
        await firstDb.SaveChangesAsync();
        await secondDb.SaveChangesAsync();

        var firstProviderValue = await ReadProviderValueAsync(
            firstDb,
            "ContactRequests",
            "Message",
            firstContact.Id);
        var secondProviderValue = await ReadProviderValueAsync(
            secondDb,
            "ContactRequests",
            "Message",
            secondContact.Id);
        firstDb.ChangeTracker.Clear();
        secondDb.ChangeTracker.Clear();
        var firstRoundTrip = await firstDb.ContactRequests.SingleAsync();
        var secondRoundTrip = await secondDb.ContactRequests.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(firstProtector.KeyIdentifier, Is.Not.EqualTo(secondProtector.KeyIdentifier));
            Assert.That(firstDb.Model, Is.Not.SameAs(secondDb.Model),
                "converter-bearing EF models must not cross protection-key boundaries");
            Assert.That(firstProtector.Unprotect(firstProviderValue), Is.EqualTo(firstPlaintext));
            Assert.That(secondProtector.Unprotect(secondProviderValue), Is.EqualTo(secondPlaintext));
            Assert.Throws<AuthenticationTagMismatchException>(() =>
                firstProtector.Unprotect(secondProviderValue));
            Assert.Throws<AuthenticationTagMismatchException>(() =>
                secondProtector.Unprotect(firstProviderValue));
            Assert.That(firstRoundTrip.Message, Is.EqualTo(firstPlaintext));
            Assert.That(secondRoundTrip.Message, Is.EqualTo(secondPlaintext));
        });
    }

    [Test]
    public async Task Legacy_plaintext_is_backfilled_for_all_scoped_records_and_tampering_fails()
    {
        // Prime SQLite's AppDbContext model cache with a different key. Without a key-aware cache
        // boundary, the target context below captures this protector's converters and the first
        // post-backfill materialization fails authentication in full-suite ordering.
        await using (var primeConnection = new SqliteConnection("Data Source=:memory:"))
        {
            await primeConnection.OpenAsync();
            await using var primeDb = CreateRelationalDb(
                primeConnection,
                CreateProtector("legacy-prime-protection-key-32-characters"));
            _ = primeDb.Model;
        }

        var application = new YouthApplication
        {
            FirstName = "Encrypted first",
            LastName = "Encrypted last",
            Age = 15,
            WhoIsFilling = "guardian",
            Neighbourhood = "Encrypted neighbourhood",
            CohortInterest = "Encrypted cohort",
            WhyInterested = "Encrypted application narrative",
            TrackInterest = "Encrypted track",
            GuardianName = "Encrypted guardian",
            GuardianEmail = "encrypted.guardian@example.com",
            GuardianPhone = "+1 416 555 0100",
            ParticipationConsentAck = true,
            PrivacyAck = true,
            DecisionNote = "Encrypted decision",
        };
        var contact = new ContactRequest
        {
            Name = "Encrypted contact",
            Email = "encrypted.contact@example.com",
            Topic = "Encrypted topic",
            Message = "Encrypted contact narrative",
        };
        var volunteer = new VolunteerSignup
        {
            Name = "Encrypted volunteer",
            Email = "encrypted.volunteer@example.com",
            Phone = "+1 416 555 0101",
            AreasOfInterest = "Encrypted volunteer interests",
            ScreeningConsentAck = true,
        };
        var donation = new DonationPledge
        {
            DonorName = "Encrypted donor",
            Email = "encrypted.donor@example.com",
            Tier = "monthly-25",
            MonthlyAmount = 25,
            ProviderReference = "provider-secret-reference",
        };
        var dataRequest = new DataSubjectRequest
        {
            Reference = $"DSR-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
            SubjectUserId = Guid.NewGuid(),
            ActionType = "correction",
            Details = "Encrypted request detail",
            RequestedDisplayName = "Encrypted requested name",
            RequestedEmail = "encrypted.requested@example.com",
            RequestedPronouns = "they/them",
            ResolutionNote = "Encrypted resolution note",
        };
        var auditEntry = new AuditEntry
        {
            Action = "SensitiveDataBackfillTest",
            Source = "ip=192.0.2.1 | user-agent=legacy agent",
            Details = "Legacy audit detail with personal narrative",
        };
        var notification = new Notification
        {
            UserId = Guid.NewGuid(),
            EventKey = "legacy-notification",
            Title = "Legacy notification title",
            Body = "Legacy notification personal body",
        };
        var scheduledNotification = new ScheduledNotification
        {
            UserId = Guid.NewGuid(),
            EventKey = "legacy-scheduled",
            Title = "Legacy scheduled title",
            Body = "Legacy scheduled personal body",
            SourceType = "backfill-test",
            SourceId = Guid.NewGuid(),
            DeliverAt = DateTime.UtcNow.AddDays(1),
        };
        var userFile = new StoredUserFile
        {
            OwnerUserId = Guid.NewGuid(),
            Purpose = StoredUserFilePurpose.SupportScreenshot,
            OriginalFileName = "private-user-file.png",
            ContentType = "image/png",
            SizeBytes = 5,
            Sha256 = new string('a', 64),
            Content = [1, 3, 5, 7, 9],
            CreatedAt = DateTime.UtcNow,
        };
        var submissionFile = new StoredSubmissionFile
        {
            OwnerUserId = Guid.NewGuid(),
            AssignmentId = Guid.NewGuid(),
            Kind = SubmissionFileKind.File,
            OriginalFileName = "private-submission-file.txt",
            ContentType = "text/plain",
            SizeBytes = 6,
            Sha256 = new string('b', 64),
            Content = [2, 4, 6, 8, 10, 12],
        };

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var protector = CreateProtector();
        await using var db = CreateRelationalDb(connection, protector);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            application,
            contact,
            volunteer,
            donation,
            dataRequest,
            auditEntry,
            notification,
            scheduledNotification,
            userFile,
            submissionFile);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // Simulate an upgrade from the historical plaintext schema by bypassing EF converters.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [Applications] SET
                [FirstName] = {application.FirstName}, [LastName] = {application.LastName},
                [WhoIsFilling] = {application.WhoIsFilling}, [Neighbourhood] = {application.Neighbourhood},
                [CohortInterest] = {application.CohortInterest}, [WhyInterested] = {application.WhyInterested},
                [TrackInterest] = {application.TrackInterest}, [GuardianName] = {application.GuardianName},
                [GuardianEmail] = {application.GuardianEmail}, [GuardianPhone] = {application.GuardianPhone},
                [DecisionNote] = {application.DecisionNote}
            WHERE [Id] = {application.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [ContactRequests] SET [Name] = {contact.Name}, [Email] = {contact.Email},
                [Topic] = {contact.Topic}, [Message] = {contact.Message}
            WHERE [Id] = {contact.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [VolunteerSignups] SET [Name] = {volunteer.Name}, [Email] = {volunteer.Email},
                [Phone] = {volunteer.Phone}, [AreasOfInterest] = {volunteer.AreasOfInterest}
            WHERE [Id] = {volunteer.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [DonationPledges] SET [DonorName] = {donation.DonorName}, [Email] = {donation.Email},
                [ProviderReference] = {donation.ProviderReference}
            WHERE [Id] = {donation.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [DataSubjectRequests] SET [Details] = {dataRequest.Details},
                [RequestedDisplayName] = {dataRequest.RequestedDisplayName},
                [RequestedEmail] = {dataRequest.RequestedEmail},
                [RequestedPronouns] = {dataRequest.RequestedPronouns},
                [ResolutionNote] = {dataRequest.ResolutionNote}
            WHERE [Id] = {dataRequest.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [AuditEntries] SET [Source] = {auditEntry.Source}, [Details] = {auditEntry.Details}
            WHERE [Id] = {auditEntry.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [Notifications] SET [Title] = {notification.Title}, [Body] = {notification.Body}
            WHERE [Id] = {notification.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [ScheduledNotifications] SET [Title] = {scheduledNotification.Title},
                [Body] = {scheduledNotification.Body}
            WHERE [Id] = {scheduledNotification.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [StoredUserFiles] SET [OriginalFileName] = {userFile.OriginalFileName},
                [Content] = {userFile.Content}
            WHERE [Id] = {userFile.Id}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [StoredSubmissionFiles] SET [OriginalFileName] = {submissionFile.OriginalFileName},
                [Content] = {submissionFile.Content}
            WHERE [Id] = {submissionFile.Id}
            """);

        var legacyText = await ReadProviderValueAsync(
            db,
            "ContactRequests",
            "Message",
            contact.Id);
        var legacyBytes = await ReadProviderBytesAsync(
            db,
            "StoredUserFiles",
            "Content",
            userFile.Id);
        Assert.Multiple(() =>
        {
            Assert.That(legacyText, Is.EqualTo(contact.Message),
                "the raw upgrade fixture must contain plaintext, not EF provider ciphertext");
            Assert.That(legacyText, Does.Not.StartWith("enc:v1:"));
            Assert.That(legacyBytes, Is.EqualTo(userFile.Content));
            Assert.That(legacyBytes.AsSpan().StartsWith("enc:b1:"u8), Is.False);
        });

        await new SensitiveDataBackfill(db, protector).RunAsync();

        var protectedFields = new[]
        {
            ("Applications", "FirstName", application.Id, application.FirstName),
            ("Applications", "WhyInterested", application.Id, application.WhyInterested),
            ("Applications", "GuardianEmail", application.Id, application.GuardianEmail!),
            ("Applications", "DecisionNote", application.Id, application.DecisionNote!),
            ("ContactRequests", "Name", contact.Id, contact.Name),
            ("ContactRequests", "Email", contact.Id, contact.Email),
            ("ContactRequests", "Message", contact.Id, contact.Message),
            ("VolunteerSignups", "Phone", volunteer.Id, volunteer.Phone!),
            ("VolunteerSignups", "AreasOfInterest", volunteer.Id, volunteer.AreasOfInterest!),
            ("DonationPledges", "DonorName", donation.Id, donation.DonorName),
            ("DonationPledges", "ProviderReference", donation.Id, donation.ProviderReference!),
            ("DataSubjectRequests", "Details", dataRequest.Id, dataRequest.Details!),
            ("DataSubjectRequests", "ResolutionNote", dataRequest.Id, dataRequest.ResolutionNote!),
            ("AuditEntries", "Source", auditEntry.Id, auditEntry.Source!),
            ("AuditEntries", "Details", auditEntry.Id, auditEntry.Details!),
            ("Notifications", "Title", notification.Id, notification.Title),
            ("Notifications", "Body", notification.Id, notification.Body),
            ("ScheduledNotifications", "Title", scheduledNotification.Id, scheduledNotification.Title),
            ("ScheduledNotifications", "Body", scheduledNotification.Id, scheduledNotification.Body),
            ("StoredUserFiles", "OriginalFileName", userFile.Id, userFile.OriginalFileName),
            ("StoredSubmissionFiles", "OriginalFileName", submissionFile.Id, submissionFile.OriginalFileName),
        };
        foreach (var (table, column, id, plaintext) in protectedFields)
        {
            var providerValue = await ReadProviderValueAsync(db, table, column, id);
            Assert.Multiple(() =>
            {
                Assert.That(providerValue, Does.StartWith("enc:v1:"), $"{table}.{column}");
                Assert.That(providerValue, Does.Not.Contain(plaintext), $"{table}.{column}");
            });
        }
        foreach (var (table, id, plaintext) in new[]
        {
            ("StoredUserFiles", userFile.Id, userFile.Content),
            ("StoredSubmissionFiles", submissionFile.Id, submissionFile.Content),
        })
        {
            var providerValue = await ReadProviderBytesAsync(db, table, "Content", id);
            Assert.Multiple(() =>
            {
                Assert.That(providerValue.AsSpan().StartsWith("enc:b1:"u8), Is.True, table);
                Assert.That(providerValue.AsSpan().IndexOf(plaintext), Is.EqualTo(-1), table);
            });
        }

        db.ChangeTracker.Clear();
        var roundTripApplication = await db.Applications.SingleAsync(item => item.Id == application.Id);
        var roundTripContact = await db.ContactRequests.SingleAsync(item => item.Id == contact.Id);
        var roundTripVolunteer = await db.VolunteerSignups.SingleAsync(item => item.Id == volunteer.Id);
        var roundTripDonation = await db.DonationPledges.SingleAsync(item => item.Id == donation.Id);
        var roundTripAudit = await db.AuditEntries.SingleAsync(item => item.Id == auditEntry.Id);
        var roundTripNotification = await db.Notifications.SingleAsync(item => item.Id == notification.Id);
        var roundTripScheduled = await db.ScheduledNotifications.SingleAsync(item => item.Id == scheduledNotification.Id);
        var roundTripUserFile = await db.StoredUserFiles.SingleAsync(item => item.Id == userFile.Id);
        var roundTripSubmissionFile = await db.StoredSubmissionFiles.SingleAsync(item => item.Id == submissionFile.Id);
        Assert.Multiple(() =>
        {
            Assert.That(roundTripApplication.WhyInterested, Is.EqualTo(application.WhyInterested));
            Assert.That(roundTripApplication.GuardianEmail, Is.EqualTo(application.GuardianEmail));
            Assert.That(roundTripContact.Message, Is.EqualTo(contact.Message));
            Assert.That(roundTripVolunteer.AreasOfInterest, Is.EqualTo(volunteer.AreasOfInterest));
            Assert.That(roundTripDonation.ProviderReference, Is.EqualTo(donation.ProviderReference));
            Assert.That(roundTripAudit.Source, Is.EqualTo(auditEntry.Source));
            Assert.That(roundTripAudit.Details, Is.EqualTo(auditEntry.Details));
            Assert.That(roundTripNotification.Body, Is.EqualTo(notification.Body));
            Assert.That(roundTripScheduled.Body, Is.EqualTo(scheduledNotification.Body));
            Assert.That(roundTripUserFile.OriginalFileName, Is.EqualTo(userFile.OriginalFileName));
            Assert.That(roundTripUserFile.Content, Is.EqualTo(userFile.Content));
            Assert.That(roundTripSubmissionFile.OriginalFileName, Is.EqualTo(submissionFile.OriginalFileName));
            Assert.That(roundTripSubmissionFile.Content, Is.EqualTo(submissionFile.Content));
        });

        var binaryCiphertext = await ReadProviderBytesAsync(db, "StoredUserFiles", "Content", userFile.Id);
        binaryCiphertext[^1] ^= 0x01;
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE [StoredUserFiles] SET [Content] = {binaryCiphertext} WHERE [Id] = {userFile.Id}");
        db.ChangeTracker.Clear();
        Assert.ThrowsAsync<AuthenticationTagMismatchException>(async () =>
            await db.StoredUserFiles.SingleAsync(item => item.Id == userFile.Id));

        var ciphertext = await ReadProviderValueAsync(db, "ContactRequests", "Message", contact.Id);
        var tampered = ciphertext.ToCharArray();
        var tamperIndex = "enc:v1:".Length + 4;
        tampered[tamperIndex] = tampered[tamperIndex] == 'A' ? 'B' : 'A';
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE [ContactRequests] SET [Message] = {new string(tampered)} WHERE [Id] = {contact.Id}");
        db.ChangeTracker.Clear();

        Assert.ThrowsAsync<AuthenticationTagMismatchException>(async () =>
            await db.ContactRequests.SingleAsync(item => item.Id == contact.Id));
    }

    private static async Task<string> ReadProviderValueAsync(
        AppDbContext db,
        string table,
        string column,
        Guid id)
    {
        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT [{column}] FROM [{table}] WHERE [Id] = @id";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = id;
            command.Parameters.Add(parameter);
            return (string)(await command.ExecuteScalarAsync())!;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    private static async Task<byte[]> ReadProviderBytesAsync(
        AppDbContext db,
        string table,
        string column,
        Guid id)
    {
        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT [{column}] FROM [{table}] WHERE [Id] = @id";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = id;
            command.Parameters.Add(parameter);
            return (byte[])(await command.ExecuteScalarAsync())!;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    private sealed class MutableClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow { get; set; } = utcNow;
    }

    private sealed class StubSmtpTransport : ISmtpEmailTransport
    {
        public bool Fail { get; init; }
        public int Attempts { get; private set; }

        public Task SendAsync(OutboundEmail message, CancellationToken cancellationToken)
        {
            Attempts++;
            if (Fail) throw new InvalidOperationException("Synthetic SMTP failure.");
            return Task.CompletedTask;
        }
    }

    private static AppDbContext CreateRelationalDb(
        SqliteConnection connection,
        ISensitiveDataProtector? protector = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        return new AppDbContext(options, protector ?? CreateProtector());
    }

    private static AesSensitiveDataProtector CreateProtector() => CreateProtector(
        "sensitive-data-backfill-tests-encryption-key");

    private static AesSensitiveDataProtector CreateProtector(string encryptionKey) => new(
        Options.Create(new SensitiveDataOptions
        {
            EncryptionKey = encryptionKey,
        }));
}

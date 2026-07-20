using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Mentoring.Flags;
using WordUp.Application.PlatformServices.Privacy;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-007 (at-risk flags), L2-AUTH-010 (mentor scoping)
[TestFixture]
[Property("Req", "L2-MENTOR-007")]
public sealed class FlagTests : IntegrationTestBase
{
    private sealed record Scenario(string MentorEmail, Guid YouthA, Guid OtherYouth);

    private async Task<Scenario> SeedMentorWithYouthAsync()
    {
        var mentorId = Guid.NewGuid();
        var otherMentorId = Guid.NewGuid();
        var youthA = Guid.NewGuid();
        var otherYouth = Guid.NewGuid();
        var mentorEmail = $"mentor.{Guid.NewGuid():N}@example.com";

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var hash = hasher.Hash("Passw0rd!");

        db.Users.AddRange(
            new User { Id = mentorId, Email = mentorEmail, DisplayName = "Mentor One", Role = Roles.Mentor, Status = UserStatus.Active, PasswordHash = hash },
            new User { Id = otherMentorId, Email = $"mentor2.{Guid.NewGuid():N}@example.com", DisplayName = "Mentor Two", Role = Roles.Mentor, Status = UserStatus.Active, PasswordHash = hash },
            new User { Id = youthA, Email = $"youtha.{Guid.NewGuid():N}@example.com", DisplayName = "Aaron Youth", Role = Roles.Youth, Status = UserStatus.Active, MentorId = mentorId, PasswordHash = hash },
            new User { Id = otherYouth, Email = $"youthc.{Guid.NewGuid():N}@example.com", DisplayName = "Carl Youth", Role = Roles.Youth, Status = UserStatus.Active, MentorId = otherMentorId, PasswordHash = hash });

        await db.SaveChangesAsync();

        return new Scenario(mentorEmail, youthA, otherYouth);
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PostAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    [Test]
    [Property("Req", "L2-MENTOR-007")]
    [Property("Req", "L2-AUTH-010")]
    public async Task Raising_a_flag_lists_the_youth_at_risk_and_resolving_removes_it()
    {
        // Traces to: L2-MENTOR-007, L2-AUTH-010
        var scenario = await SeedMentorWithYouthAsync();
        var token = await SignInAsync(scenario.MentorEmail);

        var raise = await PostAsync($"/api/mentor/youth/{scenario.YouthA}/flag",
            new { reason = "Missed two sessions" }, token);
        Assert.That(raise.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterRaise = await GetAsync("/api/mentor/flags", token);
        var atRisk = await afterRaise.Content.ReadFromJsonAsync<List<AtRiskYouth>>();
        Assert.That(atRisk!.Select(a => a.YouthId), Does.Contain(scenario.YouthA));

        Guid flagId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            flagId = await db.Flags
                .Where(f => f.YouthUserId == scenario.YouthA && f.ResolvedAt == null)
                .Select(f => f.Id)
                .SingleAsync();
        }

        var resolve = await PostAsync($"/api/flags/{flagId}/resolve",
            new { resolutionReason = FlagResolutionReasons.Reconnected }, token);
        Assert.That(resolve.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterResolve = await GetAsync("/api/mentor/flags", token);
        var stillAtRisk = await afterResolve.Content.ReadFromJsonAsync<List<AtRiskYouth>>();
        Assert.That(stillAtRisk!.Select(a => a.YouthId), Does.Not.Contain(scenario.YouthA));
    }

    [Test]
    public async Task Flag_free_text_never_enters_the_append_only_audit_and_is_erased_on_removal()
    {
        var scenario = await SeedMentorWithYouthAsync();
        var token = await SignInAsync(scenario.MentorEmail);
        var reasonSentinel = $"PRIVATE-FLAG-REASON-{Guid.NewGuid():N}";

        var raise = await PostAsync(
            $"/api/mentor/youth/{scenario.YouthA}/flag",
            new { reason = reasonSentinel },
            token);
        Assert.That(raise.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var flag = await db.Flags.SingleAsync(item =>
            item.YouthUserId == scenario.YouthA && item.ResolvedAt == null);
        var raisedAudit = await db.AuditEntries.SingleAsync(item => item.Action == "YouthFlagged");
        Assert.Multiple(() =>
        {
            Assert.That(raisedAudit.TargetType, Is.EqualTo("Flag"));
            Assert.That(raisedAudit.TargetId, Is.EqualTo(flag.Id));
            Assert.That(raisedAudit.Details, Is.EqualTo("category=manual"));
            Assert.That(raisedAudit.Details, Does.Not.Contain(reasonSentinel));
        });

        var youth = await db.Users.SingleAsync(item => item.Id == scenario.YouthA);
        await scope.ServiceProvider.GetRequiredService<PersonalDataAnonymizer>()
            .AnonymizeAsync(youth, DateTime.UtcNow, CancellationToken.None);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var erasedFlag = await db.Flags.SingleAsync(item => item.Id == flag.Id);
        var relatedAuditEntries = await db.AuditEntries.AsNoTracking()
            .Where(item => item.TargetId == flag.Id || item.TargetId == scenario.YouthA)
            .ToListAsync();
        var auditText = relatedAuditEntries
            .Select(item => (item.Details ?? string.Empty) + " " + (item.Source ?? string.Empty))
            .ToList();
        Assert.Multiple(() =>
        {
            Assert.That(erasedFlag.Reason, Is.EqualTo("[removed]"));
            Assert.That(auditText.Any(item => item.Contains(reasonSentinel, StringComparison.Ordinal)), Is.False);
        });
    }

    [Test]
    public async Task Cross_account_risk_delivery_is_content_free_and_subject_linked_email_is_purged()
    {
        var youthNameSentinel = $"PRIVATE-YOUTH-NAME-{Guid.NewGuid():N}";
        var mentor = await SeedUserAsync(Roles.Mentor, "Risk delivery mentor");
        var youth = await SeedUserAsync(
            Roles.Youth,
            youthNameSentinel,
            mentorId: mentor.Id,
            lastSignInAt: DateTime.UtcNow.AddDays(-13));
        Guid notificationId;
        Guid scheduledId;
        string reasonSentinel;

        using (var scope = Factory.Services.CreateScope())
        {
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            configuration[ConfigurationAtRiskThresholds.InactiveDaysKey] = "7";
            configuration[ConfigurationAtRiskThresholds.MissedSessionsKey] = "99";
            configuration[ConfigurationAtRiskThresholds.ProgressGapKey] = "100";
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = mentor.Id,
                EventKey = "at-risk-alert",
                InApp = true,
                Email = true,
            });
            await db.SaveChangesAsync();

            await using (var evaluationTransaction = await db.Database.BeginTransactionAsync())
            {
                await scope.ServiceProvider.GetRequiredService<AtRiskEvaluator>()
                    .EvaluateAsync(new[] { youth.Id });
                await evaluationTransaction.CommitAsync();
            }
            var notification = await db.Notifications.SingleAsync(item =>
                item.UserId == mentor.Id && item.EventKey == "at-risk-alert");
            notificationId = notification.Id;
            reasonSentinel = (await db.Flags.SingleAsync(item => item.YouthUserId == youth.Id)).Reason;
            var capturedEmail = Factory.Emails.LastTo(mentor.Email)!;
            Assert.Multiple(() =>
            {
                Assert.That(notification.Title + notification.Body, Does.Not.Contain(youthNameSentinel));
                Assert.That(notification.Title + notification.Body, Does.Not.Contain(reasonSentinel));
                Assert.That(capturedEmail.Subject + capturedEmail.Body, Does.Not.Contain(youthNameSentinel));
                Assert.That(capturedEmail.Subject + capturedEmail.Body, Does.Not.Contain(reasonSentinel));
                Assert.That(capturedEmail.SubjectUserId, Is.EqualTo(youth.Id));
            });

            var scheduled = new ScheduledNotification
            {
                UserId = mentor.Id,
                EventKey = "at-risk-follow-up",
                Title = notification.Title,
                Body = notification.Body,
                LinkPath = notification.LinkPath,
                SourceType = "at-risk-flag",
                SourceId = Guid.NewGuid(),
                DeliverAt = DateTime.UtcNow.AddDays(1),
            };
            db.ScheduledNotifications.Add(scheduled);
            scheduledId = scheduled.Id;
            await db.SaveChangesAsync();

            var outbox = new OutboxEmailSender(
                db,
                scope.ServiceProvider.GetRequiredService<IClock>(),
                scope.ServiceProvider.GetRequiredService<IUserPrivacyLock>());
            await using var privacyTransaction = await db.Database.BeginTransactionAsync();
            await outbox.SendAsync(
                mentor.Email,
                capturedEmail.Subject,
                capturedEmail.Body,
                subjectUserId: youth.Id);
            await privacyTransaction.CommitAsync();

            var trackedYouth = await db.Users.SingleAsync(item => item.Id == youth.Id);
            await scope.ServiceProvider.GetRequiredService<PersonalDataAnonymizer>()
                .AnonymizeAsync(trackedYouth, DateTime.UtcNow, CancellationToken.None);
            await db.SaveChangesAsync();
        }

        using var verifyScope = Factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var retainedNotification = await verify.Notifications.FindAsync(notificationId);
        var retainedScheduled = await verify.ScheduledNotifications.FindAsync(scheduledId);
        var subjectLinkedEmails = await verify.OutboundEmails.AsNoTracking()
            .Where(item => item.SubjectUserId == youth.Id)
            .ToListAsync();
        var retainedText = new[]
            {
                (retainedNotification?.Title ?? string.Empty) + " " + (retainedNotification?.Body ?? string.Empty),
                (retainedScheduled?.Title ?? string.Empty) + " " + (retainedScheduled?.Body ?? string.Empty),
            }
            .Concat(subjectLinkedEmails.Select(item => item.Subject + " " + item.Body))
            .ToList();
        Assert.Multiple(() =>
        {
            Assert.That(retainedNotification, Is.Not.Null);
            Assert.That(retainedScheduled, Is.Not.Null);
            Assert.That(verify.OutboundEmails.Any(item => item.SubjectUserId == youth.Id), Is.False);
            Assert.That(retainedText.Any(item => item.Contains(youthNameSentinel, StringComparison.Ordinal)), Is.False);
            Assert.That(retainedText.Any(item => item.Contains(reasonSentinel, StringComparison.Ordinal)), Is.False);
        });
    }

    [Test]
    [Property("Req", "L2-MENTOR-007")]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Automatic_signals_use_configured_thresholds_deduplicate_and_rearm_after_clearing()
    {
        // Traces to: L2-MENTOR-007 AC1/AC3/AC4; L2-ENGAGE-008; L2-PLATFORM-003 AC2
        var mentor = await SeedUserAsync(Roles.Mentor, "Risk Mentor");
        var director = await SeedUserAsync(Roles.ProgramAdministrator, "Program Director");
        var now = DateTime.UtcNow;
        var track = new Track { Name = "Risk track", Status = PublishStatus.Published };
        var cohort = new Cohort
        {
            Name = "Risk cohort",
            TrackId = track.Id,
            TermStart = now.AddMonths(-1),
            TermEnd = now.AddMonths(1),
            MentorId = mentor.Id,
        };
        var module = new Module
        {
            TrackId = track.Id,
            Name = "Risk module",
            Status = PublishStatus.Published,
        };
        var lesson = new Lesson
        {
            ModuleId = module.Id,
            Title = "Risk lesson",
            Status = PublishStatus.Published,
        };
        var session = new Session
        {
            CohortId = cohort.Id,
            MentorId = mentor.Id,
            Title = "Missed session",
            StartsAt = now.AddDays(-2),
            CreatedByUserId = mentor.Id,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Tracks.Add(track);
            db.Cohorts.Add(cohort);
            db.Modules.Add(module);
            db.Lessons.Add(lesson);
            db.Sessions.Add(session);
            db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = mentor.Id,
                EventKey = "at-risk-alert",
                InApp = false,
                Email = true,
            });
            await db.SaveChangesAsync();
        }
        var atRiskYouth = await SeedUserAsync(
            Roles.Youth,
            "Signal Youth",
            mentorId: mentor.Id,
            cohortId: cohort.Id,
            lastSignInAt: now.AddDays(-8));
        var peers = new[]
        {
            await SeedUserAsync(Roles.Youth, "Peer One", mentorId: mentor.Id, cohortId: cohort.Id),
            await SeedUserAsync(Roles.Youth, "Peer Two", mentorId: mentor.Id, cohortId: cohort.Id),
            await SeedUserAsync(Roles.Youth, "Peer Three", mentorId: mentor.Id, cohortId: cohort.Id),
        };
        Guid attendanceId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            foreach (var peer in peers)
            {
                db.LessonProgress.Add(new LessonProgress
                {
                    YouthUserId = peer.Id,
                    LessonId = lesson.Id,
                    Status = ProgressStatus.Completed,
                    CompletedAt = now.AddDays(-1),
                });
            }
            var attendance = new AttendanceRecord
            {
                SessionId = session.Id,
                YouthUserId = atRiskYouth.Id,
                Status = AttendanceStatus.Absent,
                RecordedByUserId = mentor.Id,
                RecordedAt = now.AddDays(-2),
            };
            attendanceId = attendance.Id;
            db.AttendanceRecords.Add(attendance);

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            configuration[ConfigurationAtRiskThresholds.InactiveDaysKey] = "30";
            configuration[ConfigurationAtRiskThresholds.MissedSessionsKey] = "2";
            configuration[ConfigurationAtRiskThresholds.ProgressGapKey] = "80";
            await db.SaveChangesAsync();
        }

        var mentorToken = await SignInAsync(mentor.Email);
        var directorToken = await SignInAsync(director.Email);
        var aboveConfiguredThresholds = await GetAsync("/api/mentor/flags", mentorToken);
        Assert.That(
            await aboveConfiguredThresholds.Content.ReadFromJsonAsync<List<AtRiskYouth>>(),
            Is.Empty);

        using (var scope = Factory.Services.CreateScope())
        {
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            configuration[ConfigurationAtRiskThresholds.InactiveDaysKey] = "7";
            configuration[ConfigurationAtRiskThresholds.MissedSessionsKey] = "1";
            configuration[ConfigurationAtRiskThresholds.ProgressGapKey] = "15";
        }

        var evaluated = await GetAsync("/api/mentor/flags", mentorToken);
        var flags = await evaluated.Content.ReadFromJsonAsync<List<AtRiskYouth>>();
        var automatic = flags!.Single(flag => flag.YouthId == atRiskYouth.Id);
        Assert.Multiple(() =>
        {
            Assert.That(automatic.Automatic, Is.True);
            Assert.That(automatic.FlaggedByName, Is.EqualTo("Automated evaluation"));
            Assert.That(automatic.Reasons.Single(), Does.Contain("Inactive for 8 days"));
            Assert.That(automatic.Reasons.Single(), Does.Contain("Missed 1 session"));
            Assert.That(automatic.Reasons.Single(), Does.Contain("below the cohort average"));
            Assert.That(Factory.Changes.Changes.Any(item =>
                item.Audience == "user"
                && item.AudienceId == mentor.Id
                && item.EventName == "notification"
                && JsonSerializer.Serialize(item.Payload).Contains("at-risk-alert")), Is.False);
            var email = Factory.Emails.LastTo(mentor.Email);
            Assert.That(email, Is.Not.Null, "the assigned mentor receives the automatic risk alert");
            Assert.That(email!.Subject, Is.EqualTo("Youth needs follow-up"));
            Assert.That(email.Body, Does.Contain("/mentor/flags"));
        });

        await GetAsync("/api/mentor/flags", mentorToken);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(
                await db.Flags.CountAsync(flag => flag.YouthUserId == atRiskYouth.Id && flag.ResolvedAt == null),
                Is.EqualTo(1));
            Assert.That(
                await db.AuditEntries.CountAsync(entry => entry.Action == "YouthAutomaticallyFlagged"),
                Is.EqualTo(1));
            Assert.That(
                await db.AuditEntries
                    .Where(entry => entry.Action == "YouthAutomaticallyFlagged")
                    .Select(entry => entry.IsPrivate)
                    .SingleAsync(),
                Is.True);
            Assert.That(await db.Notifications.AnyAsync(item =>
                item.UserId == mentor.Id && item.EventKey == "at-risk-alert"), Is.False);
            Assert.That(Factory.Emails.AllTo(mentor.Email), Has.Count.EqualTo(1),
                "re-evaluation does not duplicate an active risk alert");
        }

        var directorView = await GetAsync("/api/mentor/flags", directorToken);
        var directorFlags = await directorView.Content.ReadFromJsonAsync<List<AtRiskYouth>>();
        Assert.That(directorFlags!.Select(flag => flag.YouthId), Does.Contain(atRiskYouth.Id));

        var resolve = await PostAsync(
            $"/api/flags/{automatic.FlagId}/resolve",
            new { resolutionReason = FlagResolutionReasons.FlaggedInError },
            mentorToken);
        Assert.That(resolve.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var suppressed = await GetAsync("/api/mentor/flags", mentorToken);
        Assert.That(
            (await suppressed.Content.ReadFromJsonAsync<List<AtRiskYouth>>())!
                .Select(flag => flag.YouthId),
            Does.Not.Contain(atRiskYouth.Id));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Users.FindAsync(atRiskYouth.Id))!.LastSignInAt = DateTime.UtcNow;
            (await db.AttendanceRecords.FindAsync(attendanceId))!.Status = AttendanceStatus.Present;
            db.LessonProgress.Add(new LessonProgress
            {
                YouthUserId = atRiskYouth.Id,
                LessonId = lesson.Id,
                Status = ProgressStatus.Completed,
                CompletedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }
        await GetAsync("/api/mentor/flags", mentorToken);

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.AttendanceRecords.FindAsync(attendanceId))!.Status = AttendanceStatus.Absent;
            await db.SaveChangesAsync();
        }
        var recurred = await GetAsync("/api/mentor/flags", mentorToken);
        var recurringFlags = await recurred.Content.ReadFromJsonAsync<List<AtRiskYouth>>();
        var recurring = recurringFlags!.Single(flag => flag.YouthId == atRiskYouth.Id);
        Assert.Multiple(() =>
        {
            Assert.That(recurring.FlagId, Is.Not.EqualTo(automatic.FlagId));
            Assert.That(recurring.Reasons.Single(), Does.Contain("Missed 1 session"));
        });
    }

    private async Task<User> SeedUserAsync(
        string role,
        string displayName,
        Guid? mentorId = null,
        Guid? cohortId = null,
        DateTime? lastSignInAt = null)
    {
        var user = new User
        {
            Email = $"{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com",
            DisplayName = displayName,
            Role = role,
            Status = UserStatus.Active,
            MentorId = mentorId,
            CohortId = cohortId,
            LastSignInAt = lastSignInAt,
        };
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        user.PasswordHash = hasher.Hash("Passw0rd!");
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}

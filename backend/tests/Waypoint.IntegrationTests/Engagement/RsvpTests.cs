using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Engagement;

// Acceptance Test
// Traces to: L2-ENGAGE-005 (session RSVP: youth responds, optional note delivered to mentor, one RSVP per youth)
[TestFixture]
[Property("Req", "L2-ENGAGE-005")]
public sealed class RsvpTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        if (token is not null)
        {
            Authorize(request, token);
        }
        return await Client.SendAsync(request);
    }

    private async Task<(Guid Id, string Email)> SeedUserAsync(
        string role,
        Guid? mentorId = null,
        Guid? cohortId = null)
    {
        var email = $"seed.{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = email,
            DisplayName = $"Seed {role}",
            Role = role,
            Status = UserStatus.Active,
            MentorId = mentorId,
            CohortId = cohortId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task<Guid> SeedSessionAsync(Guid cohortId, int daysFromNow = 2)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = new Session
        {
            CohortId = cohortId,
            Title = "Weekly Session",
            StartsAt = DateTime.UtcNow.AddDays(daysFromNow),
            CreatedByUserId = Guid.NewGuid(),
        };
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session.Id;
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-ENGAGE-005")]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Youth_rsvps_with_a_note_and_reminder_the_mentor_sees_it_and_re_rsvp_updates_in_place()
    {
        // Traces to: L2-ENGAGE-005; L2-ENGAGE-008 AC1/AC4; L2-PLATFORM-003 AC2
        var mentor = await SeedUserAsync(Roles.Mentor);
        Guid cohortId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cohort = new Cohort
            {
                Name = "Youth cohort",
                MentorId = mentor.Id,
                TermStart = DateTime.UtcNow.AddDays(-30),
                TermEnd = DateTime.UtcNow.AddDays(90),
            };
            db.Cohorts.Add(cohort);
            await db.SaveChangesAsync();
            cohortId = cohort.Id;
        }
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id, cohortId: cohortId);
        var sessionId = await SeedSessionAsync(cohortId);
        var otherSessionId = await SeedSessionAsync(Guid.NewGuid());
        await SeedSessionAsync(cohortId, daysFromNow: -1);
        var youthToken = await SignInAsync(youth.Email);
        var mentorToken = await SignInAsync(mentor.Email);

        var before = await SendAsync(HttpMethod.Get, "/api/me/schedule", token: youthToken);
        Assert.That(before.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var upcoming = await before.Content.ReadFromJsonAsync<List<YouthSessionDto>>();
        Assert.Multiple(() =>
        {
            Assert.That(upcoming, Has.Count.EqualTo(1), "only future sessions in the youth cohort are visible");
            Assert.That(upcoming![0].SessionId, Is.EqualTo(sessionId));
            Assert.That(upcoming[0].Response, Is.Null);
        });

        var crossCohort = await SendAsync(HttpMethod.Post, $"/api/sessions/{otherSessionId}/rsvp", body: new
        {
            response = "going",
            reminderSet = false,
        }, token: youthToken);
        Assert.That(crossCohort.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var rsvp = await SendAsync(HttpMethod.Post, $"/api/sessions/{sessionId}/rsvp", body: new
        {
            response = "going",
            note = "Bringing a friend from school.",
            reminderSet = true,
        }, token: youthToken);
        Assert.That(rsvp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var current = await SendAsync(HttpMethod.Get, "/api/me/schedule", token: youthToken);
        var currentView = (await current.Content.ReadFromJsonAsync<List<YouthSessionDto>>())!.Single();
        Assert.Multiple(() =>
        {
            Assert.That(currentView.Response, Is.EqualTo("Going"));
            Assert.That(currentView.Note, Is.EqualTo("Bringing a friend from school."));
            Assert.That(currentView.ReminderSet, Is.True);
            Assert.That(currentView.ReminderAt, Is.Not.Null);
        });

        var list = await SendAsync(HttpMethod.Get, $"/api/sessions/{sessionId}/rsvps", token: mentorToken);
        Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var views = await list.Content.ReadFromJsonAsync<List<RsvpDto>>();
        var mine = views!.Single(v => v.YouthUserId == youth.Id);
        Assert.That(mine.Response, Is.EqualTo("Going"));
        Assert.That(mine.Note, Is.EqualTo("Bringing a friend from school."));
        Assert.That(mine.ReminderSet, Is.True);

        // Re-submitting updates the existing RSVP rather than creating a duplicate.
        var change = await SendAsync(HttpMethod.Post, $"/api/sessions/{sessionId}/rsvp", body: new
        {
            response = "can't make it",
            note = "Something came up.",
            reminderSet = false,
        }, token: youthToken);
        Assert.That(change.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var after = await SendAsync(HttpMethod.Get, $"/api/sessions/{sessionId}/rsvps", token: mentorToken);
        var afterViews = await after.Content.ReadFromJsonAsync<List<RsvpDto>>();
        var updated = afterViews!.Where(v => v.YouthUserId == youth.Id).ToList();
        Assert.That(updated.Count, Is.EqualTo(1), "re-RSVP must update in place, not add a row");
        Assert.That(updated[0].Response, Is.EqualTo("CantAttend"));
        Assert.That(updated[0].Note, Is.EqualTo("Something came up."));
        Assert.That(updated[0].ReminderSet, Is.False);

        using (var assertionScope = Factory.Services.CreateScope())
        {
            var assertionDb = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var rows = assertionDb.SessionRsvps.Count(r => r.SessionId == sessionId && r.YouthUserId == youth.Id);
            var reminder = assertionDb.ScheduledNotifications.Single(item =>
                item.UserId == youth.Id && item.SourceId == updated[0].Id);
            Assert.Multiple(() =>
            {
                Assert.That(rows, Is.EqualTo(1));
                Assert.That(reminder.CancelledAt, Is.Not.Null, "turning the reminder off cancels its pending delivery");
            });
        }

        // A due reminder is dispatched once into the youth notification feed.
        var reminderSessionId = await SeedSessionAsync(cohortId, daysFromNow: 3);
        var setReminder = await SendAsync(HttpMethod.Post, $"/api/sessions/{reminderSessionId}/rsvp", body: new
        {
            response = "maybe",
            reminderSet = true,
        }, token: youthToken);
        setReminder.EnsureSuccessStatusCode();
        using (var dueScope = Factory.Services.CreateScope())
        {
            var dueDb = dueScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var scheduled = dueDb.ScheduledNotifications.Single(item =>
                item.UserId == youth.Id && item.CancelledAt == null && item.DeliveredAt == null);
            scheduled.DeliverAt = DateTime.UtcNow.AddMinutes(-1);
            await dueDb.SaveChangesAsync();
            var processor = dueScope.ServiceProvider.GetRequiredService<ScheduledNotificationProcessor>();
            Assert.That(await processor.RunOnceAsync(), Is.EqualTo(1));
        }

        // Feed reads are projections only; the independent worker has already delivered the due row.
        var notificationFeed = await SendAsync(HttpMethod.Get, "/api/notifications", token: youthToken);
        var notificationFeedAgain = await SendAsync(HttpMethod.Get, "/api/notifications", token: youthToken);
        var reminders = (await notificationFeed.Content.ReadFromJsonAsync<List<NotificationDto>>())!
            .Where(item => item.EventKey == "session-reminders")
            .ToList();
        var remindersAgain = (await notificationFeedAgain.Content.ReadFromJsonAsync<List<NotificationDto>>())!
            .Where(item => item.EventKey == "session-reminders")
            .ToList();
        Assert.Multiple(() =>
        {
            Assert.That(reminders, Has.Count.EqualTo(1));
            Assert.That(reminders[0].LinkPath, Is.EqualTo("/youth/schedule"));
            Assert.That(remindersAgain, Has.Count.EqualTo(1), "a delivered schedule is consumed exactly once");
            Assert.That(Factory.Changes.Changes.Count(item =>
                item.Audience == "user"
                && item.AudienceId == youth.Id
                && item.EventName == "notification"
                && JsonSerializer.Serialize(item.Payload).Contains("session-reminders")), Is.EqualTo(1));
        });

        var optedOutSessionId = await SeedSessionAsync(cohortId, daysFromNow: 4);
        var optedOutReminder = await SendAsync(HttpMethod.Post, $"/api/sessions/{optedOutSessionId}/rsvp", body: new
        {
            response = "going",
            reminderSet = true,
        }, token: youthToken);
        optedOutReminder.EnsureSuccessStatusCode();
        using (var preferenceScope = Factory.Services.CreateScope())
        {
            var preferenceDb = preferenceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            preferenceDb.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = youth.Id,
                EventKey = "session-reminders",
                InApp = false,
                Email = true,
            });
            var scheduled = preferenceDb.ScheduledNotifications.Single(item =>
                item.UserId == youth.Id && item.DeliveredAt == null && item.CancelledAt == null);
            scheduled.DeliverAt = DateTime.UtcNow.AddMinutes(-1);
            await preferenceDb.SaveChangesAsync();
            Assert.That(
                await preferenceScope.ServiceProvider.GetRequiredService<ScheduledNotificationProcessor>()
                    .RunOnceAsync(),
                Is.EqualTo(1));
        }

        // The feed reflects channel preferences after the independent worker has run.
        var optedOutFeed = await SendAsync(HttpMethod.Get, "/api/notifications", token: youthToken);
        var optedOutReminders = (await optedOutFeed.Content.ReadFromJsonAsync<List<NotificationDto>>())!
            .Count(item => item.EventKey == "session-reminders");
        var reminderEmail = Factory.Emails.LastTo(youth.Email);
        Assert.Multiple(() =>
        {
            Assert.That(optedOutReminders, Is.EqualTo(1), "in-app opt-out suppresses the later due reminder");
            Assert.That(reminderEmail, Is.Not.Null, "the enabled email channel receives the due reminder");
            Assert.That(reminderEmail!.Subject, Is.EqualTo("Session reminder"));
            Assert.That(reminderEmail.Body, Does.Contain("/youth/schedule"));
        });
    }

    private sealed record RsvpDto(Guid Id, Guid YouthUserId, string Response, string? Note, bool ReminderSet);

    private sealed record YouthSessionDto(
        Guid SessionId,
        string Title,
        DateTime StartsAt,
        int? DurationMinutes,
        string? Location,
        string? Response,
        string? Note,
        bool ReminderSet,
        DateTime? ReminderAt);

    private sealed record NotificationDto(
        Guid Id, string EventKey, string Title, string Body, string? LinkPath, bool Read, DateTime CreatedAt);
}

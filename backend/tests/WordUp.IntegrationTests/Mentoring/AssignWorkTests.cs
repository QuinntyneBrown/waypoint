using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Mentoring.AssignWork;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-008 (catalog, recipient/cohort targeting, journey, message, reminder, count),
//            L2-ENGAGE-008 (preference-aware assignment notifications), L2-AUTH-010 (mentor scoping)
[TestFixture]
[Property("Req", "L2-MENTOR-008")]
public sealed class AssignWorkTests : IntegrationTestBase
{
    private sealed record Scenario(
        User Mentor,
        User OtherMentor,
        User YouthA,
        User YouthB,
        User OtherYouth,
        Cohort Cohort,
        Cohort OtherCohort,
        Module Module,
        Lesson Lesson,
        Lesson DraftLesson,
        Assignment Project);

    [Test]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Selected_youth_receive_journey_work_message_and_due_reminder()
    {
        // Traces to: L2-MENTOR-008 AC1..AC4, L2-ENGAGE-008, L2-PLATFORM-003 AC2
        var scenario = await SeedScenarioAsync();
        var mentorToken = await SignInAsync(scenario.Mentor.Email);

        using (var preferenceScope = Factory.Services.CreateScope())
        {
            var preferenceDb = preferenceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            preferenceDb.NotificationPreferences.AddRange(
                new NotificationPreference
                {
                    UserId = scenario.YouthB.Id,
                    EventKey = "mentor-messages",
                    InApp = false,
                    Email = true,
                },
                new NotificationPreference
                {
                    UserId = scenario.YouthB.Id,
                    EventKey = "lesson-reminders",
                    InApp = false,
                });
            await preferenceDb.SaveChangesAsync();
        }

        var optionsResponse = await SendAsync(
            HttpMethod.Get,
            "/api/mentor/assign-work/options",
            mentorToken);
        var options = await optionsResponse.Content.ReadFromJsonAsync<AssignWorkOptions>();
        Assert.Multiple(() =>
        {
            Assert.That(options!.Youth.Select(youth => youth.YouthId),
                Is.EquivalentTo(new[] { scenario.YouthA.Id, scenario.YouthB.Id }));
            Assert.That(options.Cohorts.Single().YouthCount, Is.EqualTo(2));
            Assert.That(options.Items.Select(item => item.ItemId), Does.Contain(scenario.Lesson.Id));
            Assert.That(options.Items.Select(item => item.ItemId), Does.Contain(scenario.Project.Id));
            Assert.That(options.Items.Select(item => item.ItemId), Does.Not.Contain(scenario.DraftLesson.Id));
        });

        var due = DateTime.UtcNow.AddDays(1);
        const string message = "Please finish this before our next session.";
        var assign = await SendAsync(HttpMethod.Post, "/api/mentor/assign-work", mentorToken, new
        {
            itemType = AssignWorkItemTypes.Lesson,
            itemId = scenario.Lesson.Id,
            youthIds = new[] { scenario.YouthA.Id, scenario.YouthB.Id },
            dueDate = due,
            reminderCadence = ReminderCadences.TwoDaysBefore,
            message,
        });
        Assert.That(assign.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await assign.Content.ReadFromJsonAsync<AssignWorkResult>();
        Assert.That(result!.Created, Is.EqualTo(2));

        var email = Factory.Emails.LastTo(scenario.YouthB.Email);
        Assert.Multiple(() =>
        {
            Assert.That(email, Is.Not.Null, "the email-only youth receives the assigned-work message");
            Assert.That(email!.Subject, Is.EqualTo("New assigned work"));
            Assert.That(email.Body, Does.Contain("/youth/messages"));
        });

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var work = await db.AssignedWork
                .Where(item => item.ItemId == scenario.Lesson.Id)
                .ToListAsync();
            Assert.Multiple(() =>
            {
                Assert.That(work, Has.Count.EqualTo(2));
                Assert.That(work.All(item => item.ReminderCadence == ReminderCadences.TwoDaysBefore), Is.True);
                Assert.That(work.All(item => item.Message == message), Is.True);
                Assert.That(db.Messages.Count(item => item.Body == message), Is.EqualTo(2));
                Assert.That(db.ScheduledNotifications.Count(item =>
                    item.SourceType == "AssignedWork" && item.EventKey == "lesson-reminders"),
                    Is.EqualTo(2));
                Assert.That(db.Notifications.Count(item =>
                    item.EventKey == "mentor-messages" && item.Title == "New assigned work"),
                    Is.EqualTo(1));
            });
        }

        var youthToken = await SignInAsync(scenario.YouthA.Email);
        var home = await SendAsync(HttpMethod.Get, "/api/me/home", youthToken);
        var homeBody = await home.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(homeBody, Does.Contain(scenario.Lesson.Title));
            Assert.That(homeBody, Does.Contain(message));
        });
        var thread = await SendAsync(HttpMethod.Get, "/api/me/messages", youthToken);
        Assert.That(await thread.Content.ReadAsStringAsync(), Does.Contain(message));
        var notifications = await SendAsync(HttpMethod.Get, "/api/notifications", youthToken);
        var notificationBody = await notifications.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(notificationBody, Does.Contain("New assigned work"));
            Assert.That(notificationBody, Does.Contain("Work due soon"));
            Assert.That(notificationBody, Does.Contain($"/youth/lessons/{scenario.Lesson.Id}"));
        });

        var optedOutYouthToken = await SignInAsync(scenario.YouthB.Email);
        var optedOutNotifications = await SendAsync(
            HttpMethod.Get,
            "/api/notifications",
            optedOutYouthToken);
        var optedOutBody = await optedOutNotifications.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(optedOutBody, Does.Not.Contain("New assigned work"));
            Assert.That(optedOutBody, Does.Not.Contain("Work due soon"));
        });
    }

    [Test]
    [Property("Req", "L2-AUTH-010")]
    public async Task Whole_cohort_is_counted_and_invalid_or_unscoped_targets_create_nothing()
    {
        // Traces to: L2-MENTOR-008 AC1/AC4, L2-AUTH-010
        var scenario = await SeedScenarioAsync();
        var token = await SignInAsync(scenario.Mentor.Email);

        var cohortAssign = await SendAsync(HttpMethod.Post, "/api/mentor/assign-work", token, new
        {
            itemType = AssignWorkItemTypes.Module,
            itemId = scenario.Module.Id,
            cohortId = scenario.Cohort.Id,
        });
        var result = await cohortAssign.Content.ReadFromJsonAsync<AssignWorkResult>();
        Assert.That(result!.Created, Is.EqualTo(2));

        var unscoped = await SendAsync(HttpMethod.Post, "/api/mentor/assign-work", token, new
        {
            itemType = AssignWorkItemTypes.Module,
            itemId = scenario.Module.Id,
            cohortId = scenario.OtherCohort.Id,
        });
        Assert.That(unscoped.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var missingItem = await SendAsync(HttpMethod.Post, "/api/mentor/assign-work", token, new
        {
            itemType = AssignWorkItemTypes.Lesson,
            itemId = Guid.NewGuid(),
            youthIds = new[] { scenario.YouthA.Id },
        });
        Assert.That(missingItem.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var invalidCadence = await SendAsync(HttpMethod.Post, "/api/mentor/assign-work", token, new
        {
            itemType = AssignWorkItemTypes.Lesson,
            itemId = scenario.Lesson.Id,
            youthIds = new[] { scenario.YouthA.Id },
            reminderCadence = "sometimes",
        });
        Assert.That(invalidCadence.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var unknownRecipient = await SendAsync(HttpMethod.Post, "/api/mentor/assign-work", token, new
        {
            itemType = AssignWorkItemTypes.Lesson,
            itemId = scenario.Lesson.Id,
            youthIds = new[] { scenario.YouthA.Id, Guid.NewGuid() },
        });
        Assert.That(unknownRecipient.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(db.AssignedWork.Count(item => item.CohortId == scenario.Cohort.Id), Is.EqualTo(2));
            Assert.That(db.AssignedWork.Any(item => item.YouthUserId == scenario.OtherYouth.Id), Is.False);
            Assert.That(db.AuditEntries.Count(entry =>
                entry.Action == "WorkAssigned" && entry.TargetId == scenario.Module.Id), Is.EqualTo(1));
        });
    }

    private async Task<Scenario> SeedScenarioAsync()
    {
        var now = DateTime.UtcNow;
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var hash = hasher.Hash("Passw0rd!");
        var mentor = User(Roles.Mentor, "Assign Mentor", hash);
        var otherMentor = User(Roles.Mentor, "Other Mentor", hash);
        var track = new Track { Name = "Assign track", Status = PublishStatus.Published };
        var otherTrack = new Track { Name = "Other track", Status = PublishStatus.Published };
        var cohort = new Cohort
        {
            Name = "Assign cohort",
            TrackId = track.Id,
            MentorId = mentor.Id,
            TermStart = now.AddMonths(-1),
            TermEnd = now.AddMonths(2),
        };
        var otherCohort = new Cohort
        {
            Name = "Other cohort",
            TrackId = otherTrack.Id,
            MentorId = otherMentor.Id,
            TermStart = now.AddMonths(-1),
            TermEnd = now.AddMonths(2),
        };
        var youthA = User(Roles.Youth, "Youth A", hash, mentor.Id, cohort.Id);
        var youthB = User(Roles.Youth, "Youth B", hash, mentor.Id, cohort.Id);
        var otherYouth = User(Roles.Youth, "Other Youth", hash, otherMentor.Id, otherCohort.Id);
        var module = new Module
        {
            TrackId = track.Id,
            Name = "Service module",
            Status = PublishStatus.Published,
        };
        var lesson = new Lesson
        {
            ModuleId = module.Id,
            Title = "Serve your neighbour",
            Status = PublishStatus.Published,
        };
        var draftLesson = new Lesson
        {
            ModuleId = module.Id,
            Title = "Draft lesson",
            Status = PublishStatus.Draft,
        };
        var project = new Assignment
        {
            ModuleId = module.Id,
            Title = "Service project",
            TaskDescription = "Plan an act of service.",
            SubmissionTypes = "text",
        };
        db.Users.AddRange(mentor, otherMentor, youthA, youthB, otherYouth);
        db.Tracks.AddRange(track, otherTrack);
        db.Cohorts.AddRange(cohort, otherCohort);
        db.Modules.Add(module);
        db.Lessons.AddRange(lesson, draftLesson);
        db.Assignments.Add(project);
        await db.SaveChangesAsync();
        return new Scenario(
            mentor,
            otherMentor,
            youthA,
            youthB,
            otherYouth,
            cohort,
            otherCohort,
            module,
            lesson,
            draftLesson,
            project);
    }

    private static User User(
        string role,
        string name,
        string passwordHash,
        Guid? mentorId = null,
        Guid? cohortId = null) => new()
        {
            Email = $"{name.Replace(" ", string.Empty).ToLowerInvariant()}.{Guid.NewGuid():N}@example.com",
            DisplayName = name,
            Role = role,
            Status = UserStatus.Active,
            PasswordHash = passwordHash,
            MentorId = mentorId,
            CohortId = cohortId,
            CreatedAt = DateTime.UtcNow,
        };

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new
        {
            email,
            password = "Passw0rd!",
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResult>())!.AccessToken;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        string token,
        object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }
}

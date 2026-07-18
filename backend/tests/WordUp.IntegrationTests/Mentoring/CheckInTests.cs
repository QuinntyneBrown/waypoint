using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-006 (check-ins: full journal, private-by-default, explicit shared views),
//            L2-AUTH-010 (no-IDOR: mentor may only check in on an assigned youth),
//            L2-CONSENT-003 (private note content stays out of shared views/audit)
[TestFixture]
[Property("Req", "L2-MENTOR-006")]
public sealed class CheckInTests : IntegrationTestBase
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

    private async Task<(Guid Id, string Email)> SeedUserAsync(string role, Guid? mentorId = null)
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
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-MENTOR-006")]
    public async Task Logging_a_check_in_defaults_private_and_the_mentor_history_contains_the_journal()
    {
        // Traces to: L2-MENTOR-006
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);
        var youthToken = await SignInAsync(youth.Email);

        const string prayer = "Praying for their upcoming exams.";
        var log = await SendAsync(HttpMethod.Post, "/api/mentor/check-ins", body: new
        {
            youthId = youth.Id,
            date = DateTime.UtcNow,
            type = "1:1",
            engagement = "Engaged and positive.",
            goalsDiscussed = "Finish module 2.",
            prayerRequests = prayer,
            actionItems = "Send reading list.",
            nextFollowUp = DateTime.UtcNow.AddDays(7),
            shareWithYouthAndGuardian = false,
            guardianInformed = false,
        }, token: mentorToken);
        Assert.That(log.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await log.Content.ReadFromJsonAsync<CheckInResultDto>();
        Assert.That(result!.Id, Is.Not.EqualTo(Guid.Empty));

        // Stored Private because sharing was not enabled.
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = db.CheckIns.Single(c => c.Id == result.Id);
            Assert.That(stored.Visibility, Is.EqualTo(Visibility.Private));
            Assert.That(stored.PrayerRequests, Is.EqualTo(prayer));
        }

        var historyResponse = await SendAsync(HttpMethod.Get, $"/api/mentor/check-ins?youthId={youth.Id}", token: mentorToken);
        Assert.That(historyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var history = await historyResponse.Content.ReadFromJsonAsync<List<CheckInSummaryDto>>();
        var entry = history!.Single(h => h.Id == result.Id);
        Assert.Multiple(() =>
        {
            Assert.That(entry.YouthUserId, Is.EqualTo(youth.Id));
            Assert.That(entry.YouthName, Is.EqualTo("Seed Youth"));
            Assert.That(entry.Type, Is.EqualTo("1:1"));
            Assert.That(entry.Engagement, Is.EqualTo("Engaged and positive."));
            Assert.That(entry.GoalsDiscussed, Is.EqualTo("Finish module 2."));
            Assert.That(entry.PrayerRequests, Is.EqualTo(prayer));
            Assert.That(entry.ActionItems, Is.EqualTo("Send reading list."));
            Assert.That(entry.Visibility, Is.EqualTo("Private"));
            Assert.That(entry.GuardianInformed, Is.False);
        });

        var youthProgress = await SendAsync(HttpMethod.Get, "/api/me/progress", token: youthToken);
        Assert.That(await youthProgress.Content.ReadAsStringAsync(), Does.Not.Contain(prayer));
    }

    [Test]
    [Property("Req", "L2-MENTOR-006")]
    public async Task A_mentor_cannot_log_a_check_in_for_a_youth_that_is_not_assigned_to_them()
    {
        // Traces to: L2-MENTOR-006 (no-IDOR / L2-AUTH-010)
        var mentor = await SeedUserAsync(Roles.Mentor);
        var otherMentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: otherMentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);

        var log = await SendAsync(HttpMethod.Post, "/api/mentor/check-ins", body: new
        {
            youthId = youth.Id,
            date = DateTime.UtcNow,
            type = "1:1",
            shareWithYouthAndGuardian = false,
            guardianInformed = false,
        }, token: mentorToken);
        Assert.That(log.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-MENTOR-006")]
    public async Task Explicit_sharing_reaches_youth_and_guardian_but_private_entries_do_not()
    {
        // Traces to: L2-MENTOR-006 AC2/AC3/AC4/AC5
        var mentor = await SeedUserAsync(Roles.Mentor);
        var guardian = await SeedUserAsync(Roles.Guardian);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var storedYouth = await db.Users.FindAsync(youth.Id);
            storedYouth!.GuardianUserId = guardian.Id;
            await db.SaveChangesAsync();
        }
        var mentorToken = await SignInAsync(mentor.Email);
        var youthToken = await SignInAsync(youth.Email);
        var guardianToken = await SignInAsync(guardian.Email);
        const string sharedPrayer = "Shared prayer for confidence.";
        const string privateSecret = "PRIVATE CHECK-IN SECRET";

        var shared = await SendAsync(HttpMethod.Post, "/api/mentor/check-ins", body: new
        {
            youthId = youth.Id,
            date = DateTime.UtcNow,
            type = "call",
            engagement = "Hopeful",
            goalsDiscussed = "Lead the welcome",
            prayerRequests = sharedPrayer,
            actionItems = "Draft the welcome",
            nextFollowUp = DateTime.UtcNow.AddDays(3),
            shareWithYouthAndGuardian = true,
            guardianInformed = true,
        }, token: mentorToken);
        Assert.That(shared.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var privateLog = await SendAsync(HttpMethod.Post, "/api/mentor/check-ins", body: new
        {
            youthId = youth.Id,
            date = DateTime.UtcNow.AddMinutes(-1),
            type = "1:1",
            prayerRequests = privateSecret,
            shareWithYouthAndGuardian = false,
            guardianInformed = false,
        }, token: mentorToken);
        Assert.That(privateLog.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var youthProgress = await SendAsync(HttpMethod.Get, "/api/me/progress", token: youthToken);
        var youthBody = await youthProgress.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(youthBody, Does.Contain(sharedPrayer));
            Assert.That(youthBody, Does.Not.Contain(privateSecret));
        });
        var guardianProgress = await SendAsync(
            HttpMethod.Get,
            $"/api/guardian/children/{youth.Id}/progress",
            token: guardianToken);
        var guardianBody = await guardianProgress.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(guardianBody, Does.Contain(sharedPrayer));
            Assert.That(guardianBody, Does.Not.Contain(privateSecret));
        });
        var filtered = await SendAsync(
            HttpMethod.Get,
            $"/api/mentor/check-ins?youthId={youth.Id}&type=call",
            token: mentorToken);
        var history = await filtered.Content.ReadFromJsonAsync<List<CheckInSummaryDto>>();
        Assert.That(history, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(history![0].PrayerRequests, Is.EqualTo(sharedPrayer));
            Assert.That(history[0].GuardianInformed, Is.True);
            Assert.That(history[0].Visibility, Is.EqualTo("SharedWithMentor"));
        });
    }

    private sealed record CheckInResultDto(Guid Id);

    private sealed record CheckInSummaryDto(
        Guid Id,
        Guid YouthUserId,
        string YouthName,
        DateTime Date,
        string Type,
        string? Engagement,
        string? GoalsDiscussed,
        string? PrayerRequests,
        string? ActionItems,
        string Visibility,
        DateTime? NextFollowUp,
        bool GuardianInformed,
        DateTime CreatedAt);
}

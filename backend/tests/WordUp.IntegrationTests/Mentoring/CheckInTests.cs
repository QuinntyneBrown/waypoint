using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-006 (check-ins: log, private-by-default, history without mentor-only note content),
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
    public async Task Logging_a_check_in_stores_it_private_and_history_omits_prayer_requests()
    {
        // Traces to: L2-MENTOR-006
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);

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
            shareWithMentor = false,
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
        var body = await historyResponse.Content.ReadAsStringAsync();
        // The summary must not leak mentor-only note content.
        Assert.That(body, Does.Not.Contain(prayer));

        var history = await historyResponse.Content.ReadFromJsonAsync<List<CheckInSummaryDto>>();
        var entry = history!.Single(h => h.Id == result.Id);
        Assert.That(entry.YouthUserId, Is.EqualTo(youth.Id));
        Assert.That(entry.Type, Is.EqualTo("1:1"));
        Assert.That(entry.Visibility, Is.EqualTo("Private"));
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
            shareWithMentor = false,
            guardianInformed = false,
        }, token: mentorToken);
        Assert.That(log.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private sealed record CheckInResultDto(Guid Id);

    private sealed record CheckInSummaryDto(
        Guid Id, Guid YouthUserId, DateTime Date, string Type, string Visibility, DateTime? NextFollowUp);
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.AssignWork;
using WordUp.Application.Auth;
using WordUp.Application.Flags;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-007 (at-risk flags), L2-MENTOR-008 (assign work), L2-AUTH-010 (mentor scoping)
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
    [Property("Req", "L2-MENTOR-008")]
    [Property("Req", "L2-AUTH-010")]
    public async Task Assigning_work_to_an_assigned_youth_creates_a_row_and_a_non_assigned_youth_is_forbidden()
    {
        // Traces to: L2-MENTOR-008, L2-AUTH-010
        var scenario = await SeedMentorWithYouthAsync();
        var token = await SignInAsync(scenario.MentorEmail);
        var lessonId = Guid.NewGuid();

        var assign = await PostAsync("/api/mentor/assign-work", new
        {
            itemType = "Lesson",
            itemId = lessonId,
            youthIds = new[] { scenario.YouthA },
            message = "Please complete this week",
        }, token);
        Assert.That(assign.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await assign.Content.ReadFromJsonAsync<AssignWorkResult>();
        Assert.That(result!.Created, Is.EqualTo(1));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var exists = await db.AssignedWork.AnyAsync(w =>
                w.YouthUserId == scenario.YouthA && w.ItemId == lessonId && w.ItemType == "Lesson");
            Assert.That(exists, Is.True);
        }

        var forbidden = await PostAsync("/api/mentor/assign-work", new
        {
            itemType = "Lesson",
            itemId = lessonId,
            youthIds = new[] { scenario.OtherYouth },
        }, token);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}

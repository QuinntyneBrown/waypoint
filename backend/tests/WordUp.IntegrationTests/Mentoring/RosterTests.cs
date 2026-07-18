using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Application.MentorRoster;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-001 (dashboard/roster), L2-MENTOR-002 (youth detail), L2-AUTH-010 (mentor scoping)
[TestFixture]
[Property("Req", "L2-MENTOR-001")]
public sealed class RosterTests : IntegrationTestBase
{
    private sealed record Scenario(Guid MentorId, string MentorEmail, Guid YouthA, Guid YouthB, Guid OtherYouth);

    private async Task<Scenario> SeedMentorWithYouthAsync()
    {
        var mentorId = Guid.NewGuid();
        var otherMentorId = Guid.NewGuid();
        var youthA = Guid.NewGuid();
        var youthB = Guid.NewGuid();
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
            new User { Id = youthB, Email = $"youthb.{Guid.NewGuid():N}@example.com", DisplayName = "Bianca Youth", Role = Roles.Youth, Status = UserStatus.Active, MentorId = mentorId, PasswordHash = hash },
            new User { Id = otherYouth, Email = $"youthc.{Guid.NewGuid():N}@example.com", DisplayName = "Carl Youth", Role = Roles.Youth, Status = UserStatus.Active, MentorId = otherMentorId, PasswordHash = hash });

        await db.SaveChangesAsync();

        return new Scenario(mentorId, mentorEmail, youthA, youthB, otherYouth);
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

    [Test]
    [Property("Req", "L2-MENTOR-001")]
    public async Task Dashboard_and_roster_count_only_the_mentors_assigned_youth()
    {
        // Traces to: L2-MENTOR-001, L2-AUTH-010
        var scenario = await SeedMentorWithYouthAsync();
        var token = await SignInAsync(scenario.MentorEmail);

        var dashboardResponse = await GetAsync("/api/mentor/dashboard", token);
        Assert.That(dashboardResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<MentorDashboard>();
        Assert.That(dashboard!.YouthCount, Is.EqualTo(2));

        var rosterResponse = await GetAsync("/api/mentor/youth", token);
        Assert.That(rosterResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var roster = await rosterResponse.Content.ReadFromJsonAsync<List<RosterYouth>>();

        Assert.That(roster!.Count, Is.EqualTo(2));
        var ids = roster.Select(r => r.Id).ToHashSet();
        Assert.That(ids, Does.Contain(scenario.YouthA));
        Assert.That(ids, Does.Contain(scenario.YouthB));
        Assert.That(ids, Does.Not.Contain(scenario.OtherYouth));
    }

    [Test]
    [Property("Req", "L2-MENTOR-002")]
    [Property("Req", "L2-AUTH-010")]
    public async Task Youth_detail_is_allowed_for_an_assigned_youth_and_forbidden_for_another_mentors_youth()
    {
        // Traces to: L2-MENTOR-002, L2-AUTH-010
        var scenario = await SeedMentorWithYouthAsync();
        var token = await SignInAsync(scenario.MentorEmail);

        var allowed = await GetAsync($"/api/mentor/youth/{scenario.YouthA}", token);
        Assert.That(allowed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await allowed.Content.ReadFromJsonAsync<YouthDetail>();
        Assert.That(detail!.Id, Is.EqualTo(scenario.YouthA));

        var forbidden = await GetAsync($"/api/mentor/youth/{scenario.OtherYouth}", token);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}

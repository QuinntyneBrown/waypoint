using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Engagement;

// Acceptance Test
// Traces to: L2-ENGAGE-006 (announcements: audience/role targeting, per-user reads, scheduled send, read-rate stats)
[TestFixture]
[Property("Req", "L2-ENGAGE-006")]
public sealed class AnnouncementTests : IntegrationTestBase
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

    private async Task<(Guid Id, string Email)> SeedUserAsync(string role, Guid? cohortId = null)
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
            CohortId = cohortId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task<Guid> SeedCohortAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cohort = new Cohort
        {
            Name = $"Cohort {Guid.NewGuid():N}",
            TermStart = DateTime.UtcNow,
            TermEnd = DateTime.UtcNow.AddMonths(3),
        };
        db.Cohorts.Add(cohort);
        await db.SaveChangesAsync();
        return cohort.Id;
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-ENGAGE-006")]
    public async Task Youth_sees_only_role_targeted_announcements_and_can_mark_them_read()
    {
        // Traces to: L2-ENGAGE-006
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var forYouth = await SendAsync(HttpMethod.Post, "/api/announcements", body: new
        {
            title = "Youth night moved",
            message = "This week's youth night is at 6pm.",
            audienceScope = "org",
            roleTargeting = "Youth",
        }, token: adminToken);
        Assert.That(forYouth.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var youthAnnouncement = await forYouth.Content.ReadFromJsonAsync<AnnouncementResultDto>();
        Assert.That(youthAnnouncement!.SentAt, Is.Not.Null, "an announcement with no future schedule sends immediately");

        var forMentors = await SendAsync(HttpMethod.Post, "/api/announcements", body: new
        {
            title = "Mentor huddle",
            message = "Mentors only, Friday 8am.",
            audienceScope = "org",
            roleTargeting = "Mentors",
        }, token: adminToken);
        var mentorAnnouncement = await forMentors.Content.ReadFromJsonAsync<AnnouncementResultDto>();

        var feed = await SendAsync(HttpMethod.Get, "/api/announcements", token: youthToken);
        Assert.That(feed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var views = await feed.Content.ReadFromJsonAsync<List<AnnouncementViewDto>>();
        var visible = views!.SingleOrDefault(v => v.Id == youthAnnouncement.Id);
        Assert.That(visible, Is.Not.Null, "youth should see a Youth-targeted announcement");
        Assert.That(visible!.Read, Is.False);
        Assert.That(views!.Any(v => v.Id == mentorAnnouncement!.Id), Is.False, "youth must not see a Mentors-only announcement");

        var read = await SendAsync(HttpMethod.Post, $"/api/announcements/{youthAnnouncement.Id}/read", token: youthToken);
        Assert.That(read.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var feed2 = await SendAsync(HttpMethod.Get, "/api/announcements", token: youthToken);
        var views2 = await feed2.Content.ReadFromJsonAsync<List<AnnouncementViewDto>>();
        Assert.That(views2!.Single(v => v.Id == youthAnnouncement.Id).Read, Is.True);
    }

    [Test]
    [Property("Req", "L2-ENGAGE-006")]
    public async Task A_future_scheduled_announcement_is_not_visible_until_it_is_sent()
    {
        // Traces to: L2-ENGAGE-006
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var scheduled = await SendAsync(HttpMethod.Post, "/api/announcements", body: new
        {
            title = "Upcoming trip",
            message = "Details next week.",
            audienceScope = "org",
            roleTargeting = "Youth",
            scheduledAt = DateTime.UtcNow.AddDays(2),
        }, token: adminToken);
        var result = await scheduled.Content.ReadFromJsonAsync<AnnouncementResultDto>();
        Assert.That(result!.SentAt, Is.Null, "a future-scheduled announcement is not yet sent");

        var feed = await SendAsync(HttpMethod.Get, "/api/announcements", token: youthToken);
        var views = await feed.Content.ReadFromJsonAsync<List<AnnouncementViewDto>>();
        Assert.That(views!.Any(v => v.Id == result.Id), Is.False, "a not-yet-sent announcement must not appear in the feed");
    }

    [Test]
    [Property("Req", "L2-ENGAGE-006")]
    public async Task Stats_report_recipient_count_reads_and_read_rate()
    {
        // Traces to: L2-ENGAGE-006
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var cohortId = await SeedCohortAsync();
        var youthA = await SeedUserAsync(Roles.Youth, cohortId: cohortId);
        await SeedUserAsync(Roles.Youth, cohortId: cohortId); // second recipient, does not read
        var youthAToken = await SignInAsync(youthA.Email);

        var compose = await SendAsync(HttpMethod.Post, "/api/announcements", body: new
        {
            title = "Cohort reminder",
            message = "See you Tuesday.",
            audienceScope = "cohort",
            cohortId,
            roleTargeting = "Youth",
        }, token: adminToken);
        var announcement = await compose.Content.ReadFromJsonAsync<AnnouncementResultDto>();

        var read = await SendAsync(HttpMethod.Post, $"/api/announcements/{announcement!.Id}/read", token: youthAToken);
        Assert.That(read.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var statsResponse = await SendAsync(HttpMethod.Get, $"/api/announcements/{announcement.Id}/stats", token: adminToken);
        Assert.That(statsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var stats = await statsResponse.Content.ReadFromJsonAsync<AnnouncementStatsDto>();
        Assert.That(stats!.Recipients, Is.EqualTo(2));
        Assert.That(stats.Reads, Is.EqualTo(1));
        Assert.That(stats.ReadRate, Is.EqualTo(0.5));
    }

    private sealed record AnnouncementResultDto(Guid Id, DateTime? SentAt);

    private sealed record AnnouncementViewDto(Guid Id, string Title, string Message, DateTime? SentAt, bool Read);

    private sealed record AnnouncementStatsDto(int Recipients, int Reads, double ReadRate);
}

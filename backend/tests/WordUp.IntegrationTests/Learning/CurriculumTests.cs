using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-001 (curriculum browsing + gating), L2-LEARN-002 (lesson delivery/completion),
//            L2-LEARN-009 (journey map), L2-LEARN-010 (progress dashboard)
[TestFixture]
[Property("Req", "L2-LEARN-001")]
[Property("Req", "L2-LEARN-002")]
[Property("Req", "L2-LEARN-009")]
[Property("Req", "L2-LEARN-010")]
public sealed class CurriculumTests : IntegrationTestBase
{
    private sealed record Seed(
        Guid TrackId,
        Guid ModuleAId,
        Guid ModuleBId,
        Guid Lesson1Id,
        Guid Lesson2Id,
        Guid YouthId,
        string YouthToken);

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string url, object? body, string? token)
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

    /// <summary>Seeds a published track (Module A with 2 published lessons; Module B gated on A) and a
    /// youth enrolled via a cohort of that track, then signs the youth in.</summary>
    private async Task<Seed> SeedTrackAndYouthAsync()
    {
        var track = new Track { Name = "Bible", Key = $"bible-{Guid.NewGuid():N}", Status = PublishStatus.Published };
        var moduleA = new Module { TrackId = track.Id, Name = "Foundations", Order = 1, Status = PublishStatus.Published };
        var moduleB = new Module
        {
            TrackId = track.Id,
            Name = "Deeper",
            Order = 2,
            Status = PublishStatus.Published,
            PrerequisiteModuleId = moduleA.Id,
        };
        var lesson1 = new Lesson { ModuleId = moduleA.Id, Title = "Lesson 1", Order = 1, Status = PublishStatus.Published };
        var lesson2 = new Lesson { ModuleId = moduleA.Id, Title = "Lesson 2", Order = 2, Status = PublishStatus.Published };

        var email = $"youth.{Guid.NewGuid():N}@example.com";
        Guid youthId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var cohort = new Cohort { Name = "Cohort", TrackId = track.Id };
            var youth = new User
            {
                Email = email,
                DisplayName = "Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CohortId = cohort.Id,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            youthId = youth.Id;

            db.Tracks.Add(track);
            db.Modules.AddRange(moduleA, moduleB);
            db.Lessons.AddRange(lesson1, lesson2);
            db.Cohorts.Add(cohort);
            db.Users.Add(youth);
            await db.SaveChangesAsync();
        }

        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        signIn.EnsureSuccessStatusCode();
        var token = (await signIn.Content.ReadFromJsonAsync<AuthResult>())!.AccessToken;

        return new Seed(track.Id, moduleA.Id, moduleB.Id, lesson1.Id, lesson2.Id, youthId, token);
    }

    [Test]
    [Property("Req", "L2-LEARN-001")]
    public async Task Browse_curriculum_returns_published_modules_and_gates_a_module_on_its_prerequisite()
    {
        // Traces to: L2-LEARN-001
        var seed = await SeedTrackAndYouthAsync();

        var response = await SendAsync(HttpMethod.Get, $"/api/me/curriculum/{seed.TrackId}", null, seed.YouthToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var view = await response.Content.ReadFromJsonAsync<CurriculumViewDto>();
        Assert.That(view!.Modules, Has.Count.EqualTo(2));

        var moduleA = view.Modules.Single(m => m.ModuleId == seed.ModuleAId);
        Assert.That(moduleA.Locked, Is.False);
        Assert.That(moduleA.Status, Is.EqualTo("not-started"));

        var moduleB = view.Modules.Single(m => m.ModuleId == seed.ModuleBId);
        Assert.That(moduleB.Locked, Is.True, "Module B is gated on Module A which is incomplete");
        Assert.That(moduleB.Status, Is.EqualTo("locked"));
        Assert.That(moduleB.UnlockCondition, Is.Not.Null.And.Contains("Foundations"));
    }

    [Test]
    [Property("Req", "L2-LEARN-002")]
    [Property("Req", "L2-LEARN-010")]
    public async Task Completing_lessons_in_order_updates_progress_and_out_of_order_is_forbidden()
    {
        // Traces to: L2-LEARN-002, L2-LEARN-010
        var seed = await SeedTrackAndYouthAsync();

        // Completing the second lesson before its predecessor is server-side gated.
        var outOfOrder = await SendAsync(
            HttpMethod.Post, $"/api/me/lessons/{seed.Lesson2Id}/complete", null, seed.YouthToken);
        Assert.That(outOfOrder.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        // Complete lesson 1 -> progress reflects one completed lesson and overall percent rises.
        var complete1 = await SendAsync(
            HttpMethod.Post, $"/api/me/lessons/{seed.Lesson1Id}/complete", null, seed.YouthToken);
        Assert.That(complete1.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterOne = await (await SendAsync(HttpMethod.Get, "/api/me/progress", null, seed.YouthToken))
            .Content.ReadFromJsonAsync<ProgressViewDto>();
        Assert.That(afterOne!.LessonsCompleted, Is.EqualTo(1));
        Assert.That(afterOne.OverallPercent, Is.EqualTo(50).Within(0.001));

        // Now lesson 2 is unlocked.
        var complete2 = await SendAsync(
            HttpMethod.Post, $"/api/me/lessons/{seed.Lesson2Id}/complete", null, seed.YouthToken);
        Assert.That(complete2.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterTwo = await (await SendAsync(HttpMethod.Get, "/api/me/progress", null, seed.YouthToken))
            .Content.ReadFromJsonAsync<ProgressViewDto>();
        Assert.That(afterTwo!.LessonsCompleted, Is.EqualTo(2));
        Assert.That(afterTwo.OverallPercent, Is.EqualTo(100).Within(0.001));
    }

    [Test]
    [Property("Req", "L2-LEARN-009")]
    public async Task Journey_returns_modules_and_an_earned_badge()
    {
        // Traces to: L2-LEARN-009
        var seed = await SeedTrackAndYouthAsync();

        Guid badgeId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var badge = new Badge { Name = "First Steps", Icon = "footprints", CriteriaKind = BadgeCriteriaKind.Manual };
            badgeId = badge.Id;
            db.Badges.Add(badge);
            db.BadgeAwards.Add(new BadgeAward { BadgeId = badge.Id, YouthUserId = seed.YouthId });
            await db.SaveChangesAsync();
        }

        var response = await SendAsync(HttpMethod.Get, "/api/me/journey", null, seed.YouthToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var view = await response.Content.ReadFromJsonAsync<JourneyViewDto>();
        Assert.That(view!.Modules, Has.Count.EqualTo(2));
        Assert.That(view.Earned.Any(e => e.BadgeId == badgeId && e.Name == "First Steps"), Is.True);
    }

    [Test]
    [Property("Req", "L2-LEARN-001")]
    public async Task A_non_youth_cannot_hit_a_youth_route()
    {
        // Traces to: L2-LEARN-001
        var mentorToken = await CreateUserAndGetTokenAsync(Roles.Mentor);

        var response = await SendAsync(HttpMethod.Get, "/api/me/progress", null, mentorToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private sealed record CurriculumViewDto(IReadOnlyList<ModuleStatusDto> Modules);

    private sealed record ModuleStatusDto(
        Guid ModuleId, string Name, int Order, string Status, double CompletionPercent, bool Locked, string? UnlockCondition);

    private sealed record ProgressViewDto(
        double OverallPercent, double QuizAverage, int LessonsCompleted, double AttendanceRate,
        IReadOnlyList<CompetencyLevelDtoRow> Competencies);

    private sealed record CompetencyLevelDtoRow(Guid CompetencyId, string Level);

    private sealed record JourneyViewDto(
        IReadOnlyList<ModuleStatusDto> Modules,
        IReadOnlyList<EarnedBadgeDto> Earned,
        IReadOnlyList<LockedBadgeDto> Locked);

    private sealed record EarnedBadgeDto(Guid BadgeId, string Name, string Icon, DateTime AwardedAt);

    private sealed record LockedBadgeDto(Guid BadgeId, string Name, string Icon, string Criteria);
}

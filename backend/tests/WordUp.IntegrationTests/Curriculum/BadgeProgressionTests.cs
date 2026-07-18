using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Curriculum;

// Acceptance Test
// Traces to: L2-CURRIC-005 (badge authoring + manual award), L2-CURRIC-006 (progression authoring)
[TestFixture]
[Property("Req", "L2-CURRIC-005")]
[Property("Req", "L2-CURRIC-006")]
public sealed class BadgeProgressionTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        if (token is not null)
        {
            Authorize(request, token);
        }
        return await Client.SendAsync(request);
    }

    private sealed record IdDto(Guid Id);

    private Guid SeedYouth()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var youth = new User
        {
            Email = $"badge.youth.{Guid.NewGuid():N}@example.com",
            DisplayName = "Badge Youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            PasswordHash = "x",
        };
        db.Users.Add(youth);
        db.SaveChanges();
        return youth.Id;
    }

    [Test]
    [Property("Req", "L2-CURRIC-005")]
    public async Task Admin_creates_a_manual_badge_and_a_mentor_awards_it_once()
    {
        // Traces to: L2-CURRIC-005
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var mentorToken = await CreateUserAndGetTokenAsync(Roles.Mentor);
        var youthId = SeedYouth();

        var create = await PostAsync("/api/badges", new
        {
            name = "Faithful Attendance",
            icon = "star",
            description = "Attended every session this term.",
            criteriaKind = BadgeCriteriaKind.Manual,
            autoRule = (string?)null,
            competencyId = (Guid?)null,
        }, adminToken);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var badgeId = (await create.Content.ReadFromJsonAsync<IdDto>())!.Id;

        // A mentor awards the badge.
        var award = await PostAsync($"/api/badges/{badgeId}/award", new { youthUserId = youthId }, mentorToken);
        Assert.That(award.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Awarding the same badge to the same youth again is idempotent.
        var awardAgain = await PostAsync($"/api/badges/{badgeId}/award", new { youthUserId = youthId }, mentorToken);
        Assert.That(awardAgain.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var count = await db.BadgeAwards.CountAsync(a => a.BadgeId == badgeId && a.YouthUserId == youthId);
            Assert.That(count, Is.EqualTo(1), "a youth should hold a given badge exactly once");
        }

        // A youth may not award badges.
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await PostAsync($"/api/badges/{badgeId}/award", new { youthUserId = youthId }, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-CURRIC-006")]
    public async Task Admin_creates_a_progression_stage_and_adds_a_competency()
    {
        // Traces to: L2-CURRIC-006
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var createStage = await PostAsync("/api/progression/stages", new
        {
            name = "Rooted",
            description = "Foundational discipleship.",
            order = 0,
        }, adminToken);
        Assert.That(createStage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var stageId = (await createStage.Content.ReadFromJsonAsync<IdDto>())!.Id;

        var addCompetency = await PostAsync($"/api/progression/stages/{stageId}/competencies", new
        {
            name = "Can explain the Gospel",
        }, adminToken);
        Assert.That(addCompetency.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var competencyId = (await addCompetency.Content.ReadFromJsonAsync<IdDto>())!.Id;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(await db.ProgressionStages.AnyAsync(s => s.Id == stageId), Is.True);
            var competency = await db.Competencies.FirstOrDefaultAsync(c => c.Id == competencyId);
            Assert.That(competency, Is.Not.Null);
            Assert.That(competency!.StageId, Is.EqualTo(stageId));
        }
    }
}

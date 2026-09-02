using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Curriculum;

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

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> DeleteAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private sealed record IdDto(Guid Id);

    private Guid SeedYouth(Guid? mentorId = null)
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
            MentorId = mentorId,
        };
        db.Users.Add(youth);
        db.SaveChanges();
        return youth.Id;
    }

    private static Guid SubjectId(string accessToken)
    {
        var payload = accessToken.Split('.')[1]
            .Replace('-', '+')
            .Replace('_', '/');
        payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
        return Guid.Parse(document.RootElement.GetProperty("sub").GetString()!);
    }

    [Test]
    [Property("Req", "L2-CURRIC-005")]
    public async Task Admin_creates_a_manual_badge_and_a_mentor_awards_it_once()
    {
        // Traces to: L2-CURRIC-005
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var mentorToken = await CreateUserAndGetTokenAsync(Roles.Mentor);
        var youthId = SeedYouth(SubjectId(mentorToken));

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

    [Test]
    [Property("Req", "L2-CURRIC-005")]
    public async Task Admin_authors_targeted_badge_rules_and_archiving_prevents_new_awards()
    {
        // Traces to: L2-CURRIC-005 (AC1 definition, AC4 archive)
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var mentorToken = await CreateUserAndGetTokenAsync(Roles.Mentor);
        Guid moduleId;
        Guid competencyId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var track = new Track { Name = "Badge Track", Key = $"badge-{Guid.NewGuid():N}" };
            var module = new Module { TrackId = track.Id, Name = "Badge Module", Order = 1 };
            var stage = new ProgressionStage { Name = "Badge Stage", Order = 1 };
            var competency = new Competency { StageId = stage.Id, Name = "Badge competency", Order = 1 };
            db.Tracks.Add(track);
            db.Modules.Add(module);
            db.ProgressionStages.Add(stage);
            db.Competencies.Add(competency);
            await db.SaveChangesAsync();
            moduleId = module.Id;
            competencyId = competency.Id;
        }

        var invalidRule = await PostAsync("/api/badges", new
        {
            name = "Invalid badge",
            icon = "star",
            criteriaKind = BadgeCriteriaKind.Auto,
            autoRule = "unknown-rule",
            targetRefId = moduleId,
            competencyId,
        }, adminToken);
        Assert.That(invalidRule.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var created = await PostAsync("/api/badges", new
        {
            name = "Module Finisher",
            icon = "award",
            description = "Complete the selected module.",
            criteriaKind = BadgeCriteriaKind.Auto,
            autoRule = "module-completed",
            targetRefId = moduleId,
            competencyId,
        }, adminToken);
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var badgeId = (await created.Content.ReadFromJsonAsync<IdDto>())!.Id;

        var list = await GetAsync("/api/badges", adminToken);
        Assert.That(
            list.StatusCode,
            Is.EqualTo(HttpStatusCode.OK),
            Factory.ExceptionAlerts.Alerts.LastOrDefault()?.ExceptionType
                ?? await list.Content.ReadAsStringAsync());
        var definitions = await list.Content.ReadFromJsonAsync<List<BadgeDefinitionDto>>();
        var definition = definitions!.Single(item => item.Id == badgeId);
        Assert.Multiple(() =>
        {
            Assert.That(definition.AutoRule, Is.EqualTo("module-completed"));
            Assert.That(definition.TargetRefId, Is.EqualTo(moduleId));
            Assert.That(definition.CompetencyName, Is.EqualTo("Badge competency"));
            Assert.That(definition.AwardedCount, Is.EqualTo(0));
        });

        Assert.That((await DeleteAsync($"/api/badges/{badgeId}", adminToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent));
        var youthId = SeedYouth();
        var archivedAward = await PostAsync(
            $"/api/badges/{badgeId}/award",
            new { youthUserId = youthId },
            mentorToken);
        Assert.That(archivedAward.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using var verifyScope = Factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await verifyDb.BadgeAwards.AnyAsync(item => item.BadgeId == badgeId), Is.False);
    }

    [Test]
    [Property("Req", "L2-CURRIC-006")]
    [Property("Req", "L2-MENTOR-005")]
    public async Task Admin_reads_edits_reorders_and_removes_progression_content_used_by_mentor_assessment()
    {
        // Traces to: L2-CURRIC-006 AC1-AC3
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var mentorToken = await CreateUserAndGetTokenAsync(Roles.Mentor);

        var invalidStage = await PostAsync("/api/progression/stages", new
        {
            name = " ",
            description = "Invalid",
            order = -1,
        }, adminToken);
        Assert.That(invalidStage.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var first = await PostAsync("/api/progression/stages", new
        {
            name = "Explorer Draft",
            description = "Starting the journey.",
            order = 100,
        }, adminToken);
        var second = await PostAsync("/api/progression/stages", new
        {
            name = "Grounded",
            description = "Growing in the Word.",
            order = 101,
        }, adminToken);
        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
        var firstStageId = (await first.Content.ReadFromJsonAsync<IdDto>())!.Id;
        var secondStageId = (await second.Content.ReadFromJsonAsync<IdDto>())!.Id;
        var invalidCompetency = await PostAsync(
            $"/api/progression/stages/{firstStageId}/competencies",
            new { name = " " },
            adminToken);
        Assert.That(invalidCompetency.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var removable = await PostAsync($"/api/progression/stages/{firstStageId}/competencies", new
        {
            name = "Temporary competency",
        }, adminToken);
        var retained = await PostAsync($"/api/progression/stages/{secondStageId}/competencies", new
        {
            name = "Reads Scripture independently",
        }, adminToken);
        var removableId = (await removable.Content.ReadFromJsonAsync<IdDto>())!.Id;
        var retainedId = (await retained.Content.ReadFromJsonAsync<IdDto>())!.Id;

        var initial = await GetAsync("/api/progression", adminToken);
        Assert.That(initial.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var initialStages = await initial.Content.ReadFromJsonAsync<List<ProgressionStageDto>>();
        Assert.That(initialStages!.Single(item => item.Id == firstStageId).Competencies.Single().Id,
            Is.EqualTo(removableId));
        Assert.That((await GetAsync("/api/progression", mentorToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));

        var updated = await PutAsync($"/api/progression/stages/{firstStageId}", new
        {
            name = "Explorer",
            description = "New to Scripture and community.",
        }, adminToken);
        Assert.That(updated.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var partial = await PostAsync("/api/progression/stages/reorder", new
        {
            orderedStageIds = new[] { secondStageId },
        }, adminToken);
        Assert.That(partial.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var completeOrder = initialStages!
            .Select(item => item.Id)
            .Where(id => id != firstStageId && id != secondStageId)
            .Prepend(firstStageId)
            .Prepend(secondStageId)
            .ToList();
        var reordered = await PostAsync("/api/progression/stages/reorder", new
        {
            orderedStageIds = completeOrder,
        }, adminToken);
        Assert.That(reordered.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await DeleteAsync($"/api/progression/competencies/{removableId}", adminToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent));

        var saved = await (await GetAsync("/api/progression", adminToken))
            .Content.ReadFromJsonAsync<List<ProgressionStageDto>>();
        Assert.Multiple(() =>
        {
            Assert.That(saved!.Take(2).Select(item => item.Id),
                Is.EqualTo(new[] { secondStageId, firstStageId }));
            Assert.That(saved!.Single(item => item.Id == firstStageId).Name, Is.EqualTo("Explorer"));
            Assert.That(saved!.Single(item => item.Id == firstStageId).Description,
                Is.EqualTo("New to Scripture and community."));
            Assert.That(saved!.SelectMany(item => item.Competencies).Any(item => item.Id == removableId), Is.False);
        });

        var youthId = SeedYouth(SubjectId(mentorToken));
        var mentorView = await GetAsync($"/api/mentor/youth/{youthId}/competencies", mentorToken);
        Assert.That(mentorView.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var framework = await mentorView.Content.ReadFromJsonAsync<MentorCompetencyFrameworkDto>();
        Assert.Multiple(() =>
        {
            Assert.That(framework!.Items.Single(item => item.CompetencyId == retainedId).StageName,
                Is.EqualTo("Grounded"));
            Assert.That(framework.Items.Any(item => item.CompetencyId == removableId), Is.False);
            Assert.That(framework.CurrentStageName, Is.EqualTo("Grounded"));
        });
    }

    private sealed record BadgeDefinitionDto(
        Guid Id,
        string Name,
        string Icon,
        string CriteriaKind,
        string? AutoRule,
        Guid? TargetRefId,
        Guid? CompetencyId,
        string? CompetencyName,
        int AwardedCount);

    private sealed record ProgressionStageDto(
        Guid Id,
        string Name,
        string? Description,
        int Order,
        IReadOnlyList<ProgressionCompetencyDto> Competencies);

    private sealed record ProgressionCompetencyDto(Guid Id, string Name, int Order);

    private sealed record MentorCompetencyFrameworkDto(
        string? CurrentStageName,
        IReadOnlyList<MentorCompetencyDto> Items);

    private sealed record MentorCompetencyDto(Guid CompetencyId, string StageName);
}

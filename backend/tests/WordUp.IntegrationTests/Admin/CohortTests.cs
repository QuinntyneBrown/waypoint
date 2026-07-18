using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Admin;

// Acceptance Test
// Traces to: L2-ADMIN-003 (cohort management), L2-ADMIN-004 (progress board)
[TestFixture]
[Property("Req", "L2-ADMIN-003")]
public sealed class CohortTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body, string? token)
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

    private Guid SeedTrack(PublishStatus status = PublishStatus.Published)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var track = new Track { Name = "Bible", Key = $"bible-{Guid.NewGuid():N}", Status = status, Enabled = true };
        db.Add(track);
        db.SaveChanges();
        return track.Id;
    }

    private Guid SeedUser(string role)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new User
        {
            Email = $"{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com",
            DisplayName = $"{role} Person",
            Role = role,
            Status = UserStatus.Active,
        };
        db.Add(user);
        db.SaveChanges();
        return user.Id;
    }

    [Test]
    [Property("Req", "L2-ADMIN-003")]
    public async Task Admin_creates_a_cohort_and_non_admin_is_forbidden()
    {
        // Traces to: L2-ADMIN-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts",
            new { name = "Fall 2026", trackId, leadMentorId = mentorId, location = "Lagos" }, adminToken);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await create.Content.ReadFromJsonAsync<CohortResultDto>();
        Assert.That(result!.Id, Is.Not.EqualTo(Guid.Empty));

        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await SendAsync(HttpMethod.Post, "/api/cohorts",
            new { name = "Nope", trackId, leadMentorId = mentorId }, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-ADMIN-003")]
    public async Task Adding_a_youth_to_a_cohort_is_reflected_in_the_member_count()
    {
        // Traces to: L2-ADMIN-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);
        var youthId = SeedUser(Roles.Youth);

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts",
            new { name = "Spring 2026", trackId, leadMentorId = mentorId }, adminToken);
        var cohortId = (await create.Content.ReadFromJsonAsync<CohortResultDto>())!.Id;

        var add = await SendAsync(HttpMethod.Post, $"/api/cohorts/{cohortId}/youth", new { youthId }, adminToken);
        Assert.That(add.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var list = await SendAsync(HttpMethod.Get, "/api/cohorts", null, adminToken);
        var cohorts = await list.Content.ReadFromJsonAsync<List<CohortSummaryDto>>();
        var summary = cohorts!.Single(c => c.Id == cohortId);
        Assert.That(summary.MemberCount, Is.EqualTo(1));
    }

    [Test]
    [Property("Req", "L2-ADMIN-004")]
    public async Task Progress_board_returns_youth_rows_and_published_module_columns()
    {
        // Traces to: L2-ADMIN-004
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);

        Guid moduleId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var module = new Module
            {
                TrackId = trackId,
                Name = "Genesis",
                Order = 1,
                Status = PublishStatus.Published,
            };
            db.Add(module);
            db.SaveChanges();
            moduleId = module.Id;
        }

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts",
            new { name = "Progress Cohort", trackId, leadMentorId = mentorId }, adminToken);
        var cohortId = (await create.Content.ReadFromJsonAsync<CohortResultDto>())!.Id;

        var youthId = SeedUser(Roles.Youth);
        var add = await SendAsync(HttpMethod.Post, $"/api/cohorts/{cohortId}/youth", new { youthId }, adminToken);
        Assert.That(add.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var progress = await SendAsync(HttpMethod.Get, $"/api/cohorts/{cohortId}/progress", null, adminToken);
        Assert.That(progress.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var board = await progress.Content.ReadFromJsonAsync<ProgressBoardDto>();
        Assert.That(board!.Modules.Select(m => m.ModuleId), Does.Contain(moduleId));
        Assert.That(board.Rows.Select(r => r.YouthId), Does.Contain(youthId));
        Assert.That(board.CohortAveragePercent, Is.EqualTo(0));

        var row = board.Rows.Single(r => r.YouthId == youthId);
        Assert.That(row.Cells.Single(c => c.ModuleId == moduleId).State, Is.EqualTo("NotStarted"));
    }

    private sealed record CohortResultDto(Guid Id);
    private sealed record CohortSummaryDto(Guid Id, string Name, Guid? TrackId, Guid? MentorId, int MemberCount);
    private sealed record ProgressBoardDto(
        List<ModuleColumnDto> Modules,
        List<YouthRowDto> Rows,
        double CohortAveragePercent);
    private sealed record ModuleColumnDto(Guid ModuleId, string Name);
    private sealed record YouthRowDto(
        Guid YouthId,
        string YouthName,
        List<CellStateDto> Cells,
        double QuizAverage,
        double OverallPercent);
    private sealed record CellStateDto(Guid ModuleId, string State);
}

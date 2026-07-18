using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-005 (competency assessment: set level, latest-wins, mastered/proficient count),
//            L2-AUTH-010 (no-IDOR: mentor may only assess an assigned youth)
[TestFixture]
[Property("Req", "L2-MENTOR-005")]
public sealed class CompetencyTests : IntegrationTestBase
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

    private async Task<Guid> SeedCompetencyAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stage = new ProgressionStage { Name = $"Stage {Guid.NewGuid():N}", Order = 1 };
        db.ProgressionStages.Add(stage);
        var competency = new Competency { StageId = stage.Id, Name = $"Competency {Guid.NewGuid():N}", Order = 1 };
        db.Competencies.Add(competency);
        await db.SaveChangesAsync();
        return competency.Id;
    }

    [Test]
    [Property("Req", "L2-MENTOR-005")]
    public async Task Setting_a_competency_level_is_reflected_and_latest_wins()
    {
        // Traces to: L2-MENTOR-005
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);
        var competencyId = await SeedCompetencyAsync();

        var set = await SendAsync(HttpMethod.Post, $"/api/mentor/youth/{youth.Id}/competencies",
            body: new { competencyId, level = "Proficient", notes = "Strong grasp of core ideas." }, token: mentorToken);
        Assert.That(set.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var summaryResponse = await SendAsync(HttpMethod.Get, $"/api/mentor/youth/{youth.Id}/competencies", token: mentorToken);
        Assert.That(summaryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var summary = await summaryResponse.Content.ReadFromJsonAsync<CompetencySummaryDto>();
        Assert.That(summary!.MasteredOrProficient, Is.EqualTo(1));
        var item = summary.Items.Single(i => i.CompetencyId == competencyId);
        Assert.That(item.Level, Is.EqualTo("Proficient"));
        Assert.That(item.Notes, Is.EqualTo("Strong grasp of core ideas."));

        // Setting again updates the same assessment (latest wins) rather than adding a second one.
        var reset = await SendAsync(HttpMethod.Post, $"/api/mentor/youth/{youth.Id}/competencies",
            body: new { competencyId, level = "Developing", notes = "Needs more practice." }, token: mentorToken);
        Assert.That(reset.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterResponse = await SendAsync(HttpMethod.Get, $"/api/mentor/youth/{youth.Id}/competencies", token: mentorToken);
        var after = await afterResponse.Content.ReadFromJsonAsync<CompetencySummaryDto>();
        Assert.That(after!.Items.Count(i => i.CompetencyId == competencyId), Is.EqualTo(1));
        Assert.That(after.Items.Single(i => i.CompetencyId == competencyId).Level, Is.EqualTo("Developing"));
        Assert.That(after.MasteredOrProficient, Is.EqualTo(0));
    }

    [Test]
    [Property("Req", "L2-MENTOR-005")]
    public async Task A_mentor_cannot_set_a_level_for_a_youth_that_is_not_assigned_to_them()
    {
        // Traces to: L2-MENTOR-005 (no-IDOR / L2-AUTH-010)
        var mentor = await SeedUserAsync(Roles.Mentor);
        var otherMentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: otherMentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);
        var competencyId = await SeedCompetencyAsync();

        var set = await SendAsync(HttpMethod.Post, $"/api/mentor/youth/{youth.Id}/competencies",
            body: new { competencyId, level = "Proficient", notes = (string?)null }, token: mentorToken);
        Assert.That(set.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private sealed record CompetencySummaryDto(List<CompetencyLevelItemDto> Items, int MasteredOrProficient);

    private sealed record CompetencyLevelItemDto(Guid CompetencyId, string Level, string? Notes);
}

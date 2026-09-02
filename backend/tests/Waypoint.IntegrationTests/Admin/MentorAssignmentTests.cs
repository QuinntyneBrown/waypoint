// Acceptance Tests
// Traces to: L2-ADMIN-005 (youth pairing, cohort mentor roles, caseload/load balance)
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Admin;

[TestFixture]
[Property("Req", "L2-ADMIN-005")]
public sealed class MentorAssignmentTests : IntegrationTestBase
{
    private async Task<(Guid Id, string Email)> SeedUserAsync(
        string role,
        string name,
        Guid? mentorId = null,
        UserStatus status = UserStatus.Active)
    {
        var email = $"{name.Replace(" ", ".").ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = email,
            DisplayName = name,
            Role = role,
            Status = status,
            MentorId = mentorId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResult>())!.AccessToken;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body,
        string token)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Content = JsonContent.Create(body);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    [Test]
    public async Task Youth_pairing_validates_both_roles_and_updates_the_mentor_roster_immediately()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youth = await SeedUserAsync(Roles.Youth, "Pairing Youth");
        var mentor = await SeedUserAsync(Roles.Mentor, "Pairing Mentor");
        var volunteer = await SeedUserAsync(Roles.Volunteer, "Pairing Volunteer");

        var invalidMentor = await SendAsync(
            HttpMethod.Put,
            $"/api/mentor-assignments/youth/{youth.Id}",
            new { mentorId = volunteer.Id },
            adminToken);
        Assert.That(invalidMentor.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var invalidYouth = await SendAsync(
            HttpMethod.Put,
            $"/api/mentor-assignments/youth/{volunteer.Id}",
            new { mentorId = mentor.Id },
            adminToken);
        Assert.That(invalidYouth.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var paired = await SendAsync(
            HttpMethod.Put,
            $"/api/mentor-assignments/youth/{youth.Id}",
            new { mentorId = mentor.Id },
            adminToken);
        Assert.That(paired.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var mentorToken = await SignInAsync(mentor.Email);
        using var rosterRequest = new HttpRequestMessage(HttpMethod.Get, "/api/mentor/youth");
        Authorize(rosterRequest, mentorToken);
        var rosterResponse = await Client.SendAsync(rosterRequest);
        rosterResponse.EnsureSuccessStatusCode();
        var roster = await rosterResponse.Content.ReadFromJsonAsync<List<RosterDto>>();
        Assert.That(roster!.Select(item => item.Id), Does.Contain(youth.Id));

        using var overviewRequest = new HttpRequestMessage(HttpMethod.Get, "/api/mentor-assignments");
        Authorize(overviewRequest, adminToken);
        var overviewResponse = await Client.SendAsync(overviewRequest);
        overviewResponse.EnsureSuccessStatusCode();
        var overview = await overviewResponse.Content.ReadFromJsonAsync<AssignmentOverviewDto>();
        Assert.That(overview!.Youth.Single(item => item.Id == youth.Id).MentorId, Is.EqualTo(mentor.Id));
        Assert.That(overview.Mentors.Single(item => item.MentorId == mentor.Id).Caseload, Is.EqualTo(1));
    }

    [Test]
    public async Task Lead_and_supporting_roles_are_replaced_per_cohort_without_affecting_other_cohorts()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var mentorA = await SeedUserAsync(Roles.Mentor, "Mentor Alpha");
        var mentorB = await SeedUserAsync(Roles.Mentor, "Mentor Bravo");
        var mentorC = await SeedUserAsync(Roles.Mentor, "Mentor Charlie");
        Guid cohortA;
        Guid cohortB;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var first = new Cohort { Name = "North Cohort", MentorId = mentorA.Id };
            var second = new Cohort { Name = "South Cohort", MentorId = mentorB.Id };
            db.Cohorts.AddRange(first, second);
            db.CohortMentors.AddRange(
                new CohortMentor { CohortId = first.Id, MentorId = mentorA.Id, IsLead = true },
                new CohortMentor { CohortId = second.Id, MentorId = mentorB.Id, IsLead = true });
            await db.SaveChangesAsync();
            cohortA = first.Id;
            cohortB = second.Id;
        }

        var firstUpdate = await SendAsync(
            HttpMethod.Put,
            $"/api/mentor-assignments/cohorts/{cohortA}",
            new { leadMentorId = mentorA.Id, supportingMentorIds = new[] { mentorB.Id } },
            adminToken);
        Assert.That(firstUpdate.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var secondUpdate = await SendAsync(
            HttpMethod.Put,
            $"/api/mentor-assignments/cohorts/{cohortB}",
            new { leadMentorId = mentorB.Id, supportingMentorIds = new[] { mentorA.Id, mentorC.Id } },
            adminToken);
        Assert.That(secondUpdate.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var overviewRequest = new HttpRequestMessage(HttpMethod.Get, "/api/mentor-assignments");
        Authorize(overviewRequest, adminToken);
        var overviewResponse = await Client.SendAsync(overviewRequest);
        var overview = await overviewResponse.Content.ReadFromJsonAsync<AssignmentOverviewDto>();
        var north = overview!.Cohorts.Single(item => item.Id == cohortA);
        var south = overview.Cohorts.Single(item => item.Id == cohortB);
        Assert.That(north.LeadMentorId, Is.EqualTo(mentorA.Id));
        Assert.That(north.SupportingMentors.Select(item => item.Id), Is.EquivalentTo(new[] { mentorB.Id }));
        Assert.That(south.LeadMentorId, Is.EqualTo(mentorB.Id));
        Assert.That(south.SupportingMentors.Select(item => item.Id), Is.EquivalentTo(new[] { mentorA.Id, mentorC.Id }));

        var promoteSupporting = await SendAsync(
            HttpMethod.Post,
            $"/api/cohorts/{cohortA}/edit",
            new { name = "North Cohort", leadMentorId = mentorB.Id, location = (string?)null },
            adminToken);
        Assert.That(promoteSupporting.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var auditScope = Factory.Services.CreateScope();
        var auditDb = auditScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var promotedRows = await auditDb.CohortMentors
            .Where(item => item.CohortId == cohortA)
            .ToListAsync();
        Assert.That(promotedRows.Count(item => item.MentorId == mentorB.Id), Is.EqualTo(1));
        Assert.That(promotedRows.Single(item => item.MentorId == mentorB.Id).IsLead, Is.True);
        Assert.That(await auditDb.AuditEntries.CountAsync(item => item.Action == "CohortMentorsAssigned"),
            Is.EqualTo(2));
    }

    [Test]
    public async Task Staff_activity_names_the_mentor_and_reports_target_band_metrics_and_overload()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var mentor = await SeedUserAsync(Roles.Mentor, "Loaded Mentor");
        var youthIds = new List<Guid>();
        for (var index = 0; index < 9; index++)
        {
            youthIds.Add((await SeedUserAsync(Roles.Youth, $"Load Youth {index}", mentor.Id)).Id);
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.CheckIns.AddRange(
                new CheckIn { YouthUserId = youthIds[0], MentorUserId = mentor.Id, Type = "1:1" },
                new CheckIn { YouthUserId = youthIds[1], MentorUserId = mentor.Id, Type = "call" });
            db.Submissions.Add(new Submission
            {
                YouthUserId = youthIds[0],
                AssignmentId = Guid.NewGuid(),
                ReviewedByUserId = mentor.Id,
                ReviewedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/staff-activity");
        Authorize(request, adminToken);
        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var rows = await response.Content.ReadFromJsonAsync<List<MentorLoadDto>>();
        var row = rows!.Single(item => item.MentorId == mentor.Id);
        Assert.Multiple(() =>
        {
            Assert.That(row.MentorName, Is.EqualTo("Loaded Mentor"));
            Assert.That(row.Caseload, Is.EqualTo(9));
            Assert.That(row.CheckIns, Is.EqualTo(2));
            Assert.That(row.Reviews, Is.EqualTo(1));
            Assert.That(row.TargetMinimum, Is.EqualTo(4));
            Assert.That(row.TargetMaximum, Is.EqualTo(8));
            Assert.That(row.LoadBalance, Is.EqualTo("Overloaded"));
            Assert.That(row.Overloaded, Is.True);
        });
    }

    private sealed record RosterDto(Guid Id);
    private sealed record AssignmentOverviewDto(
        IReadOnlyList<MentorLoadDto> Mentors,
        IReadOnlyList<YouthAssignmentDto> Youth,
        IReadOnlyList<CohortAssignmentDto> Cohorts);
    private sealed record MentorLoadDto(
        Guid MentorId,
        string MentorName,
        int Caseload,
        int CheckIns,
        int Reviews,
        int TargetMinimum,
        int TargetMaximum,
        string LoadBalance,
        bool Overloaded);
    private sealed record YouthAssignmentDto(Guid Id, Guid? MentorId);
    private sealed record CohortAssignmentDto(
        Guid Id,
        Guid? LeadMentorId,
        IReadOnlyList<MentorOptionDto> SupportingMentors);
    private sealed record MentorOptionDto(Guid Id, string Name);
}

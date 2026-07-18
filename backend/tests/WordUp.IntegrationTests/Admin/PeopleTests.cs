using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Admin;

// Acceptance Test
// Traces to: L2-ADMIN-001 (people directory + filtering + paging),
//            L2-ADMIN-002 (person lifecycle: deactivate/reactivate/remove/role/password-reset),
//            L2-ADMIN-005 (mentor assignment)
[TestFixture]
[Property("Req", "L2-ADMIN-002")]
public sealed class PeopleTests : IntegrationTestBase
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

    /// <summary>Seeds an active user with known credentials and returns its id + email.</summary>
    private async Task<(Guid Id, string Email, string Password)> SeedUserAsync(string role, string password = "Passw0rd!")
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
            PasswordHash = hasher.Hash(password),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email, password);
    }

    [Test]
    [Property("Req", "L2-ADMIN-001")]
    public async Task Admin_lists_people_and_can_filter_by_role_while_youth_is_forbidden()
    {
        // Traces to: L2-ADMIN-001
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youth = await SeedUserAsync(Roles.Youth);
        var mentor = await SeedUserAsync(Roles.Mentor);

        var listResponse = await SendAsync(HttpMethod.Get, "/api/people?pageSize=500", token: adminToken);
        Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var all = await listResponse.Content.ReadFromJsonAsync<PagedPeopleDto>();
        Assert.That(all!.Items.Any(p => p.Id == youth.Id), Is.True);
        Assert.That(all.Items.Any(p => p.Id == mentor.Id), Is.True);
        Assert.That(all.Total, Is.GreaterThanOrEqualTo(3));

        var youthOnly = await SendAsync(HttpMethod.Get, $"/api/people?role={Roles.Youth}&pageSize=500", token: adminToken);
        var youthList = await youthOnly.Content.ReadFromJsonAsync<PagedPeopleDto>();
        Assert.That(youthList!.Items.All(p => p.Role == Roles.Youth), Is.True);
        Assert.That(youthList.Items.Any(p => p.Id == youth.Id), Is.True);
        Assert.That(youthList.Items.Any(p => p.Id == mentor.Id), Is.False);

        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await SendAsync(HttpMethod.Get, "/api/people", token: youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-ADMIN-002")]
    public async Task Deactivating_blocks_sign_in_and_reactivating_restores_it()
    {
        // Traces to: L2-ADMIN-002
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var user = await SeedUserAsync(Roles.Mentor);

        var deactivate = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/deactivate", body: new { }, token: adminToken);
        Assert.That(deactivate.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var blocked = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email = user.Email, password = user.Password });
        Assert.That(blocked.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var reactivate = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/reactivate", body: new { }, token: adminToken);
        Assert.That(reactivate.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var allowed = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email = user.Email, password = user.Password });
        Assert.That(allowed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    [Property("Req", "L2-ADMIN-002")]
    public async Task Changing_a_role_is_reflected_on_the_person_detail()
    {
        // Traces to: L2-ADMIN-002
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var user = await SeedUserAsync(Roles.Volunteer);

        var change = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/role", body: new { role = Roles.Mentor }, token: adminToken);
        Assert.That(change.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var detailResponse = await SendAsync(HttpMethod.Get, $"/api/people/{user.Id}", token: adminToken);
        var detail = await detailResponse.Content.ReadFromJsonAsync<PersonDetailDto>();
        Assert.That(detail!.Role, Is.EqualTo(Roles.Mentor));
    }

    [Test]
    [Property("Req", "L2-ADMIN-005")]
    public async Task Assigning_a_mentor_to_a_youth_is_reflected_on_the_person_detail()
    {
        // Traces to: L2-ADMIN-005
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youth = await SeedUserAsync(Roles.Youth);
        var mentor = await SeedUserAsync(Roles.Mentor);

        var assign = await SendAsync(HttpMethod.Post, $"/api/people/{youth.Id}/assign-mentor",
            body: new { mentorId = mentor.Id }, token: adminToken);
        Assert.That(assign.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var detailResponse = await SendAsync(HttpMethod.Get, $"/api/people/{youth.Id}", token: adminToken);
        var detail = await detailResponse.Content.ReadFromJsonAsync<PersonDetailDto>();
        Assert.That(detail!.MentorId, Is.EqualTo(mentor.Id));
    }

    [Test]
    [Property("Req", "L2-ADMIN-002")]
    public async Task Sending_a_password_reset_emails_the_person()
    {
        // Traces to: L2-ADMIN-002
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var user = await SeedUserAsync(Roles.Youth);

        var reset = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/send-password-reset", body: new { }, token: adminToken);
        Assert.That(reset.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        Assert.That(Factory.Emails.LastTo(user.Email), Is.Not.Null);
    }

    private sealed record PagedPeopleDto(List<PersonSummaryDto> Items, int Total, int Page, int PageSize);

    private sealed record PersonSummaryDto(
        Guid Id, string DisplayName, string Email, string Role, string Status,
        Guid? CohortId, Guid? MentorId, DateTime? LastActive);

    private sealed record PersonDetailDto(
        Guid Id, string DisplayName, string Email, string Role, string Status,
        Guid? CohortId, Guid? MentorId, Guid? GuardianUserId, bool HasValidConsent);
}

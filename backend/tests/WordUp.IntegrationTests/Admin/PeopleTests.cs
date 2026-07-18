using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
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
        Guid cohortId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cohort = new Cohort
            {
                Name = "People filter cohort",
                TermStart = DateTime.UtcNow.Date.AddMonths(-1),
                TermEnd = DateTime.UtcNow.Date.AddMonths(6),
            };
            db.Cohorts.Add(cohort);
            (await db.Users.SingleAsync(item => item.Id == youth.Id)).CohortId = cohort.Id;
            await db.SaveChangesAsync();
            cohortId = cohort.Id;
        }

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

        var cohortOnly = await SendAsync(
            HttpMethod.Get,
            $"/api/people?role={Roles.Youth}&cohortId={cohortId}&pageSize=500",
            token: adminToken);
        var cohortList = await cohortOnly.Content.ReadFromJsonAsync<PagedPeopleDto>();
        Assert.That(cohortList!.Items.Select(person => person.Id), Is.EqualTo(new[] { youth.Id }));

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
        var signedInResponse = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { email = user.Email, password = user.Password });
        var signedIn = await signedInResponse.Content.ReadFromJsonAsync<AuthSessionDto>();

        var deactivate = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/deactivate", body: new { }, token: adminToken);
        Assert.That(deactivate.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var oldSession = await SendAsync(
            HttpMethod.Put,
            "/api/me/profile",
            new { displayName = "Should not save", email = user.Email, pronouns = (string?)null, photoUrl = (string?)null },
            signedIn!.AccessToken);
        Assert.That(oldSession.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var blocked = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email = user.Email, password = user.Password });
        Assert.That(blocked.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var reactivate = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/reactivate", body: new { }, token: adminToken);
        Assert.That(reactivate.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var revokedSessionAfterReactivation = await SendAsync(
            HttpMethod.Put,
            "/api/me/profile",
            new { displayName = "Still should not save", email = user.Email, pronouns = (string?)null, photoUrl = (string?)null },
            signedIn.AccessToken);
        Assert.That(revokedSessionAfterReactivation.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

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
        var signedInResponse = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { email = user.Email, password = user.Password });
        var signedIn = await signedInResponse.Content.ReadFromJsonAsync<AuthSessionDto>();

        var change = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/role", body: new { role = Roles.Mentor }, token: adminToken);
        Assert.That(change.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var detailResponse = await SendAsync(HttpMethod.Get, $"/api/people/{user.Id}", token: adminToken);
        var detail = await detailResponse.Content.ReadFromJsonAsync<PersonDetailDto>();
        Assert.That(detail!.Role, Is.EqualTo(Roles.Mentor));

        var staleRoleSession = await SendAsync(
            HttpMethod.Put,
            "/api/me/profile",
            new { displayName = "Should not save", email = user.Email, pronouns = (string?)null, photoUrl = (string?)null },
            signedIn!.AccessToken);
        Assert.That(staleRoleSession.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await db.AuditEntries.AnyAsync(entry =>
            entry.Action == "RoleChanged" && entry.TargetId == user.Id), Is.True);
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

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await db.AuditEntries.AnyAsync(entry =>
            entry.Action == "PasswordResetSent" && entry.TargetId == user.Id), Is.True);
    }

    [Test]
    [Property("Req", "L2-ADMIN-002")]
    public async Task Youth_detail_contains_the_required_record_sections_without_private_prayer_text()
    {
        // Traces to: L2-ADMIN-002 AC1
        var fixtureResponse = await Client.PostAsync("/api/_test/youth-curriculum", content: null);
        fixtureResponse.EnsureSuccessStatusCode();
        var fixture = await fixtureResponse.Content.ReadFromJsonAsync<SeededYouthFixtureDto>();
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var response = await SendAsync(HttpMethod.Get, $"/api/people/{fixture!.YouthId}", token: adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await response.Content.ReadFromJsonAsync<PersonDetailDto>();

        Assert.Multiple(() =>
        {
            Assert.That(detail!.DisplayName, Is.EqualTo(fixture.YouthName));
            Assert.That(detail.Email, Is.EqualTo(fixture.YouthEmail));
            Assert.That(detail.CohortName, Is.EqualTo(fixture.CohortName));
            Assert.That(detail.Guardian?.Name, Is.EqualTo(fixture.GuardianName));
            Assert.That(detail.Guardian?.Email, Is.EqualTo(fixture.GuardianEmail));
            Assert.That(detail.Consent.HasValidParticipationConsent, Is.True);
            Assert.That(detail.Progress.CompletedLessons, Is.EqualTo(1));
            Assert.That(detail.Progress.TotalLessons, Is.EqualTo(2));
            Assert.That(detail.Attendance.Items.Any(item =>
                item.SessionTitle == fixture.ProgressPresentSessionTitle && item.Status == "Present"), Is.True);
            Assert.That(detail.Notes.Any(item => item.Type == "1:1"), Is.True);
            Assert.That(detail.Notes.All(item => !item.Summary.Contains("PRIVATE PRAYER DETAIL", StringComparison.Ordinal)), Is.True);
        });
    }

    [Test]
    [Property("Req", "L2-ADMIN-002")]
    public async Task Removal_is_irreversible_excludes_active_use_and_retains_progress_records()
    {
        // Traces to: L2-ADMIN-002 AC3
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var user = await SeedUserAsync(Roles.Youth);
        var progressId = Guid.NewGuid();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.LessonProgress.Add(new LessonProgress
            {
                Id = progressId,
                YouthUserId = user.Id,
                LessonId = Guid.NewGuid(),
                Status = ProgressStatus.Completed,
            });
            await db.SaveChangesAsync();
        }

        var remove = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/remove", body: new { }, token: adminToken);
        Assert.That(remove.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var blocked = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email = user.Email, password = user.Password });
        Assert.That(blocked.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var reactivate = await SendAsync(HttpMethod.Post, $"/api/people/{user.Id}/reactivate", body: new { }, token: adminToken);
        Assert.That(reactivate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var list = await SendAsync(
            HttpMethod.Get,
            $"/api/people?search={Uri.EscapeDataString(user.Email)}",
            token: adminToken);
        var people = await list.Content.ReadFromJsonAsync<PagedPeopleDto>();
        Assert.That(people!.Items.Any(item => item.Id == user.Id), Is.False);

        var detailResponse = await SendAsync(HttpMethod.Get, $"/api/people/{user.Id}", token: adminToken);
        var detail = await detailResponse.Content.ReadFromJsonAsync<PersonDetailDto>();
        Assert.That(detail!.Status, Is.EqualTo("Removed"));

        using var verifyScope = Factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await verifyDb.LessonProgress.AnyAsync(item => item.Id == progressId), Is.True);
        Assert.That(await verifyDb.AuditEntries.AnyAsync(entry =>
            entry.Action == "AccountRemoved" && entry.TargetId == user.Id), Is.True);
    }

    private sealed record PagedPeopleDto(List<PersonSummaryDto> Items, int Total, int Page, int PageSize);

    private sealed record PersonSummaryDto(
        Guid Id, string DisplayName, string Email, string Role, string Status,
        Guid? CohortId, Guid? MentorId, DateTime? LastActive);

    private sealed record PersonDetailDto(
        Guid Id,
        string DisplayName,
        string Email,
        string Role,
        string Status,
        string? Pronouns,
        string? PhotoUrl,
        bool OnboardingCompleted,
        DateTime CreatedAt,
        DateTime? LastActive,
        Guid? CohortId,
        string? CohortName,
        Guid? MentorId,
        string? MentorName,
        Guid? GuardianUserId,
        PersonGuardianDto? Guardian,
        bool HasValidConsent,
        PersonConsentDto Consent,
        PersonProgressDto Progress,
        PersonAttendanceDto Attendance,
        IReadOnlyList<PersonNoteDto> Notes);

    private sealed record PersonGuardianDto(
        string Name,
        string Email,
        string? Relationship,
        string? Phone,
        string? EmergencyContactName,
        string? EmergencyContactPhone);

    private sealed record PersonConsentDto(
        bool HasValidParticipationConsent,
        bool ParticipationConsent,
        bool MediaConsent,
        DateTime? SignedAt,
        DateTime? ValidUntil,
        DateTime? WithdrawnAt,
        string? GuardianName,
        string? Relationship);

    private sealed record PersonProgressDto(int CompletedLessons, int TotalLessons, double Percent);

    private sealed record PersonAttendanceDto(
        int Attended,
        int Total,
        double Rate,
        IReadOnlyList<PersonAttendanceItemDto> Items);

    private sealed record PersonAttendanceItemDto(
        Guid SessionId,
        string SessionTitle,
        DateTime StartsAt,
        string Status,
        string? Note);

    private sealed record PersonNoteDto(
        Guid Id,
        DateTime Date,
        string Type,
        string MentorName,
        string Summary,
        DateTime? NextFollowUp);

    private sealed record SeededYouthFixtureDto(
        Guid YouthId,
        string YouthName,
        string YouthEmail,
        string GuardianName,
        string GuardianEmail,
        string CohortName,
        string ProgressPresentSessionTitle);

    private sealed record AuthSessionDto(string AccessToken);
}

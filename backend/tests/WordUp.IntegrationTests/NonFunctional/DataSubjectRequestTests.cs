// Acceptance Test
// Traces to: L2-NFR-003 (PII protection & data-subject rights: a data-subject request is recorded;
//            the "access" right returns only the caller's own personal data)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-003")]
public sealed class DataSubjectRequestTests : IntegrationTestBase
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

    /// <summary>Seeds an active user with known credentials and signs in, returning both its id and access token.</summary>
    private async Task<(Guid Id, string Token)> SeedUserAndSignInAsync(string role, string password = "Passw0rd!")
    {
        var email = $"dsr.{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        Guid id;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = new User
            {
                Email = email,
                DisplayName = $"DSR {role}",
                Role = role,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(password),
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            id = user.Id;
        }

        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        return (id, auth!.AccessToken);
    }

    [Test]
    [Property("Req", "L2-NFR-003")]
    public async Task Recording_a_removal_data_subject_request_writes_an_audit_entry_for_the_caller()
    {
        // Traces to: L2-NFR-003
        var (userId, token) = await SeedUserAndSignInAsync(Roles.Youth);

        var response = await SendAsync(HttpMethod.Post, "/api/me/data-requests",
            body: new { actionType = "removal", details = "Please delete my <b>account</b>." }, token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<DataSubjectRequestResultDto>();
        Assert.That(result!.Reference, Is.Not.Null.And.Not.Empty);
        Assert.That(result.ActionType, Is.EqualTo("removal"));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entry = db.AuditEntries.SingleOrDefault(
            a => a.Action == "DataSubjectRequest:removal" && a.ActorUserId == userId);
        Assert.That(entry, Is.Not.Null, "the data-subject request must be recorded as an append-only audit entry");
        Assert.That(entry!.TargetId, Is.EqualTo(userId));
        // Free-text detail is stored HTML-encoded (L2-NFR-001) — the raw markup must not survive verbatim.
        Assert.That(entry.Details, Does.Not.Contain("<b>"));
    }

    [Test]
    [Property("Req", "L2-NFR-003")]
    public async Task Access_request_returns_only_the_callers_own_personal_data_and_is_recorded()
    {
        // Traces to: L2-NFR-003
        var (userId, token) = await SeedUserAndSignInAsync(Roles.Mentor);
        var (otherId, _) = await SeedUserAndSignInAsync(Roles.Youth);

        var record = await SendAsync(HttpMethod.Post, "/api/me/data-requests",
            body: new { actionType = "access", details = (string?)null }, token: token);
        Assert.That(record.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var export = await SendAsync(HttpMethod.Get, "/api/me/data", token: token);
        Assert.That(export.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var mine = await export.Content.ReadFromJsonAsync<MyDataDto>();
        Assert.That(mine!.Id, Is.EqualTo(userId), "access export is scoped strictly to the caller");
        Assert.That(mine.Id, Is.Not.EqualTo(otherId));
        Assert.That(mine.Role, Is.EqualTo(Roles.Mentor));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var recorded = db.AuditEntries.Any(a => a.Action == "DataSubjectRequest:access" && a.ActorUserId == userId);
        Assert.That(recorded, Is.True, "the access request itself must also be recorded");
    }

    [Test]
    [Property("Req", "L2-NFR-003")]
    public async Task An_anonymous_caller_cannot_export_personal_data()
    {
        // Traces to: L2-NFR-003
        var response = await SendAsync(HttpMethod.Get, "/api/me/data");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Null_action_type_returns_validation_problem_instead_of_throwing()
    {
        var (_, token) = await SeedUserAndSignInAsync(Roles.Youth);

        var response = await SendAsync(
            HttpMethod.Post,
            "/api/me/data-requests",
            body: new { actionType = (string?)null, details = "invalid request" },
            token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Property("Req", "L2-CONSENT-003")]
    public async Task Audit_source_keeps_coarse_device_and_location_without_a_long_raw_user_agent()
    {
        var (userId, token) = await SeedUserAndSignInAsync(Roles.Youth);
        var headerSentinel = $"PRIVATE-UA-{Guid.NewGuid():N}";
        var rawUserAgent =
            $"Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) Mobile Safari "
            + headerSentinel
            + new string('x', 4096);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/me/data-requests")
        {
            Content = JsonContent.Create(new { actionType = "access", details = "Source test" }),
        };
        Authorize(request, token);
        request.Headers.TryAddWithoutValidation("User-Agent", rawUserAgent);

        using var response = await Client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var audit = db.AuditEntries.Single(item =>
            item.Action == "DataSubjectRequest:access" && item.ActorUserId == userId);
        Assert.Multiple(() =>
        {
            Assert.That(audit.Source, Does.StartWith("channel=api;device=mobile;location="));
            Assert.That(audit.Source, Does.Not.Contain(headerSentinel));
            Assert.That(audit.Source, Does.Not.Contain(rawUserAgent));
            Assert.That(audit.Source, Does.Not.Contain("iPhone"));
            Assert.That(audit.Details, Does.Not.Contain(headerSentinel));
        });
    }

    private sealed record AuthResultDto(string AccessToken);

    private sealed record DataSubjectRequestResultDto(string Reference, string ActionType, DateTime RecordedAt);

    private sealed record MyDataDto(
        Guid Id, string DisplayName, string Email, string Role, string Status,
        string? Pronouns, string? PhotoUrl, Guid? CohortId, Guid? MentorId, Guid? GuardianUserId,
        DateTime CreatedAt, DateTime? LastSignInAt);
}

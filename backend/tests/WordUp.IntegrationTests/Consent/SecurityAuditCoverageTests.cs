using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Consent;

// Acceptance Tests
// Traces to: L2-AUTH-009 AC2-AC4; L2-CONSENT-003 AC1, AC3
[TestFixture]
public sealed class SecurityAuditCoverageTests : IntegrationTestBase
{
    private sealed record AuditItemDto(
        Guid Id, Guid? ActorUserId, string ActorName, string Action, string? TargetType, Guid? TargetId,
        DateTime At, string? Source, bool IsPrivate, string? Details);

    private sealed record AuditPageDto(
        IReadOnlyList<AuditItemDto> Items,
        int Page,
        int PageSize,
        int Total,
        IReadOnlyList<object> Actors,
        IReadOnlyList<string> Actions,
        bool IntegrityValid);

    private async Task<(User User, string Token)> SeedUserAsync(
        string role,
        string displayName,
        Guid? mentorId = null)
    {
        var user = new User
        {
            Email = $"audit.{Guid.NewGuid():N}@example.com",
            DisplayName = displayName,
            Role = role,
            Status = UserStatus.Active,
            MentorId = mentorId,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            user.PasswordHash = hasher.Hash("Passw0rd!");
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var signIn = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { email = user.Email, password = "Passw0rd!" });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<AuthResult>();
        return (user, auth!.AccessToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        string token,
        object? body = null,
        string? userAgent = null)
    {
        var request = new HttpRequestMessage(method, url);
        Authorize(request, token);
        if (body is not null) request.Content = JsonContent.Create(body);
        if (userAgent is not null) request.Headers.UserAgent.ParseAdd(userAgent);
        return await Client.SendAsync(request);
    }

    [Test]
    [Property("Req", "L2-AUTH-009")]
    [Property("Req", "L2-CONSENT-003")]
    public async Task Authenticated_policy_and_resource_denials_are_audited_with_actor_action_and_source()
    {
        var (_, adminToken) = await SeedUserAsync(Roles.ProgramAdministrator, "Audit Director");
        var (youth, youthToken) = await SeedUserAsync(Roles.Youth, "Denied Youth");
        var (mentor, mentorToken) = await SeedUserAsync(Roles.Mentor, "Denied Mentor");
        var (otherYouth, _) = await SeedUserAsync(Roles.Youth, "Other Youth", Guid.NewGuid());

        var policyDenied = await SendAsync(
            HttpMethod.Get,
            "/api/audit",
            youthToken,
            userAgent: "AuditAcceptance/1.0");
        var resourceDenied = await SendAsync(
            HttpMethod.Post,
            $"/api/mentor/youth/{otherYouth.Id}/flag",
            mentorToken,
            new { reason = "Must not be persisted" },
            "AuditAcceptance/1.0");
        Assert.Multiple(() =>
        {
            Assert.That(policyDenied.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(resourceDenied.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        });

        var response = await SendAsync(
            HttpMethod.Get,
            "/api/audit?action=ProtectedActionDenied&page=1&pageSize=200",
            adminToken);
        var page = await response.Content.ReadFromJsonAsync<AuditPageDto>();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.Multiple(() =>
        {
            Assert.That(page!.Items, Has.Count.EqualTo(2));
            Assert.That(page.Items.Select(item => item.ActorUserId),
                Is.EquivalentTo(new Guid?[] { youth.Id, mentor.Id }));
            Assert.That(page.Items.All(item => item.TargetType == "Endpoint"), Is.True);
            Assert.That(page.Items.Select(item => item.Details), Is.EquivalentTo(new[]
            {
                "GET /api/audit",
                $"POST /api/mentor/youth/{otherYouth.Id}/flag",
            }));
            Assert.That(page.Items.All(item => item.Source?.Contains("ip=") == true), Is.True);
            Assert.That(page.Items.All(item =>
                item.Source?.Contains("user-agent=AuditAcceptance/1.0") == true), Is.True);
        });
    }

    [Test]
    [Property("Req", "L2-CONSENT-003")]
    public async Task Audit_entries_are_append_only_and_a_forged_chain_entry_is_detected()
    {
        var (admin, adminToken) = await SeedUserAsync(Roles.ProgramAdministrator, "Integrity Director");
        var validResponse = await SendAsync(HttpMethod.Get, "/api/audit?page=1&pageSize=200", adminToken);
        var valid = await validResponse.Content.ReadFromJsonAsync<AuditPageDto>();
        Assert.That(valid!.IntegrityValid, Is.True, "fresh logger entries form a valid integrity chain");

        Guid entryId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entry = db.AuditEntries.First(item => item.ActorUserId == admin.Id);
            entryId = entry.Id;
            entry.Action = "TamperedAction";
            Assert.ThrowsAsync<InvalidOperationException>(async () => await db.SaveChangesAsync());
        }
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AuditEntries.Remove(db.AuditEntries.First(item => item.Id == entryId));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await db.SaveChangesAsync());
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AuditEntries.Add(new AuditEntry
            {
                SequenceNumber = long.MaxValue,
                PreviousHash = "FORGED-PREVIOUS-HASH",
                IntegrityHash = "FORGED-INTEGRITY-HASH",
                Action = "ForgedAuditEntry",
                At = DateTime.UtcNow,
                Source = "ip=unknown | user-agent=forged",
            });
            await db.SaveChangesAsync();
        }

        var forgedResponse = await SendAsync(HttpMethod.Get, "/api/audit?page=1&pageSize=200", adminToken);
        var afterForgery = await forgedResponse.Content.ReadFromJsonAsync<AuditPageDto>();
        Assert.That(afterForgery!.IntegrityValid, Is.False, "a forged append breaks integrity verification");
    }
}

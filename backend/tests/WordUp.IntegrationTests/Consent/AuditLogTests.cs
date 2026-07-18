using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Consent;

// Acceptance Test
// Traces to: L2-CONSENT-003 (audit log view: admin-only, newest-first, private detail redacted)
[TestFixture]
[Property("Req", "L2-CONSENT-003")]
public sealed class AuditLogTests : IntegrationTestBase
{
    private sealed record AuditItemDto(
        Guid Id, Guid? ActorUserId, string Action, string? TargetType, Guid? TargetId,
        DateTime At, string? Source, bool IsPrivate, string? Details);

    private sealed record AuditPageDto(IReadOnlyList<AuditItemDto> Items, int Page, int PageSize, int Total);

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    [Test]
    [Property("Req", "L2-CONSENT-003")]
    public async Task Admin_can_read_the_audit_log_with_private_details_redacted_and_youth_is_forbidden()
    {
        // Traces to: L2-CONSENT-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var privateAction = $"PrivateNote.{Guid.NewGuid():N}";
        Guid privateId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AuditEntries.Add(new AuditEntry
            {
                Action = "PublicAdminAction",
                TargetType = "Test",
                At = DateTime.UtcNow.AddMinutes(-5),
                IsPrivate = false,
                Details = "visible detail",
            });
            var priv = new AuditEntry
            {
                Action = privateAction,
                TargetType = "Test",
                At = DateTime.UtcNow,
                IsPrivate = true,
                Details = "sensitive check-in note content",
            };
            db.AuditEntries.Add(priv);
            await db.SaveChangesAsync();
            privateId = priv.Id;
        }

        // A youth may not view the audit log.
        var forbidden = await GetAsync("/api/audit", youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        // An admin may, and receives items.
        var response = await GetAsync("/api/audit?page=1&pageSize=200", adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var page = await response.Content.ReadFromJsonAsync<AuditPageDto>();
        Assert.That(page, Is.Not.Null);
        Assert.That(page!.Items, Is.Not.Empty);

        // Private detail is redacted; the private entry is still listed.
        var privateItem = page.Items.FirstOrDefault(i => i.Id == privateId);
        Assert.That(privateItem, Is.Not.Null, "the private audit entry should be listed");
        Assert.That(privateItem!.IsPrivate, Is.True);
        Assert.That(privateItem.Details, Is.EqualTo("[redacted]"));
        Assert.That(privateItem.Details, Does.Not.Contain("sensitive"));
    }
}

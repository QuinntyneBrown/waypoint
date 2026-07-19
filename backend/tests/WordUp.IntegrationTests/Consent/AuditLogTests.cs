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
        Guid Id, Guid? ActorUserId, string ActorName, string Action, string? TargetType, Guid? TargetId,
        DateTime At, string? Source, bool IsPrivate, string? Details);

    private sealed record AuditActorDto(Guid Id, string Name);

    private sealed record AuditPageDto(
        IReadOnlyList<AuditItemDto> Items,
        int Page,
        int PageSize,
        int Total,
        IReadOnlyList<AuditActorDto> Actors,
        IReadOnlyList<string> Actions);

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

    [Test]
    [Property("Req", "L2-CONSENT-003")]
    public async Task Admin_can_filter_by_actor_action_and_inclusive_dates_then_page_newest_first()
    {
        // Traces to: L2-CONSENT-003 AC4
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var actor = new User
        {
            DisplayName = $"Audit actor {Guid.NewGuid():N}",
            Email = $"audit.actor.{Guid.NewGuid():N}@example.com",
            PasswordHash = "not-used",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        var action = $"FilteredAction.{Guid.NewGuid():N}";

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(actor);
            db.AuditEntries.AddRange(
                new AuditEntry { ActorUserId = actor.Id, Action = action, At = new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc) },
                new AuditEntry { ActorUserId = actor.Id, Action = action, At = new DateTime(2026, 7, 11, 15, 0, 0, DateTimeKind.Utc) },
                new AuditEntry { ActorUserId = actor.Id, Action = action, At = new DateTime(2026, 7, 12, 9, 0, 0, DateTimeKind.Utc) },
                new AuditEntry { ActorUserId = actor.Id, Action = "DifferentAction", At = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc) },
                new AuditEntry { ActorUserId = Guid.NewGuid(), Action = action, At = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc) });
            await db.SaveChangesAsync();
        }

        var firstResponse = await GetAsync(
            $"/api/audit?page=1&pageSize=1&actorUserId={actor.Id}&action={Uri.EscapeDataString(action)}&from=2026-07-10&to=2026-07-11",
            adminToken);
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var first = await firstResponse.Content.ReadFromJsonAsync<AuditPageDto>();

        Assert.That(first, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(first!.Page, Is.EqualTo(1));
            Assert.That(first.PageSize, Is.EqualTo(1));
            Assert.That(first.Total, Is.EqualTo(2), "the end date includes the whole selected day");
            Assert.That(first.Items, Has.Count.EqualTo(1));
            Assert.That(first.Items[0].At, Is.EqualTo(new DateTime(2026, 7, 11, 15, 0, 0, DateTimeKind.Utc)));
            Assert.That(first.Items[0].ActorName, Is.EqualTo(actor.DisplayName));
            Assert.That(first.Actors, Has.Some.Matches<AuditActorDto>(item => item.Id == actor.Id && item.Name == actor.DisplayName));
            Assert.That(first.Actions, Does.Contain(action));
        });

        var secondResponse = await GetAsync(
            $"/api/audit?page=2&pageSize=1&actorUserId={actor.Id}&action={Uri.EscapeDataString(action)}&from=2026-07-10&to=2026-07-11",
            adminToken);
        var second = await secondResponse.Content.ReadFromJsonAsync<AuditPageDto>();
        Assert.That(second!.Items.Single().At, Is.EqualTo(new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc)));
    }
}

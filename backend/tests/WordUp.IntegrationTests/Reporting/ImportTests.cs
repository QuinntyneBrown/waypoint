// Acceptance Test
// Traces to: L2-REPORT-004 (roster CSV preview classification, commit of Ready rows, idempotent-by-email, audit)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Reporting;

[TestFixture]
[Property("Req", "L2-REPORT-004")]
public sealed class ImportTests : IntegrationTestBase
{
    private static readonly Dictionary<string, string> Mapping = new()
    {
        ["name"] = "Name",
        ["email"] = "Email",
        ["consent"] = "Consent",
    };

    private async Task<HttpResponseMessage> PostJsonAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<string> SeedExistingYouthEmailAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var email = $"existing.youth.{Guid.NewGuid():N}@example.com";
        db.Users.Add(new User
        {
            Email = email,
            DisplayName = "Existing Youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            PasswordHash = hasher.Hash("Passw0rd!"),
        });
        await db.SaveChangesAsync();
        return email;
    }

    [Test]
    [Property("Req", "L2-REPORT-004")]
    public async Task Preview_classifies_ready_duplicate_and_needs_consent_rows()
    {
        // Traces to: L2-REPORT-004
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var existingEmail = await SeedExistingYouthEmailAsync();

        var newEmail = $"import.new.{Guid.NewGuid():N}@example.com";
        var noConsentEmail = $"import.noconsent.{Guid.NewGuid():N}@example.com";
        var csv =
            "name,email,consent\n" +
            $"Alice New,{newEmail},yes\n" +
            $"Bob Dupe,{existingEmail},yes\n" +
            $"Carol NoConsent,{noConsentEmail},no\n";

        var response = await PostJsonAsync("/api/admin/import/preview",
            new { csvContent = csv, columnMapping = Mapping }, adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var preview = await response.Content.ReadFromJsonAsync<RosterPreviewDto>();
        Assert.That(preview!.Ready, Is.EqualTo(1));
        Assert.That(preview.Duplicates, Is.EqualTo(1));
        Assert.That(preview.NeedsConsent, Is.EqualTo(1));
    }

    [Test]
    [Property("Req", "L2-REPORT-004")]
    public async Task Commit_imports_only_ready_rows_and_is_idempotent_by_email()
    {
        // Traces to: L2-REPORT-004
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var existingEmail = await SeedExistingYouthEmailAsync();

        var newEmail = $"import.commit.{Guid.NewGuid():N}@example.com";
        var noConsentEmail = $"import.commit.noconsent.{Guid.NewGuid():N}@example.com";
        var csv =
            "name,email,consent\n" +
            $"Alice New,{newEmail},yes\n" +
            $"Bob Dupe,{existingEmail},yes\n" +
            $"Carol NoConsent,{noConsentEmail},no\n";

        var first = await PostJsonAsync("/api/admin/import",
            new { csvContent = csv, columnMapping = Mapping }, adminToken);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var firstResult = await first.Content.ReadFromJsonAsync<RosterImportResultDto>();
        Assert.That(firstResult!.Imported, Is.EqualTo(1));
        Assert.That(firstResult.SkippedDuplicates, Is.EqualTo(1));
        Assert.That(firstResult.NeedsConsent, Is.EqualTo(1));

        // Committing the SAME CSV again must not create a second account (idempotent by email).
        var second = await PostJsonAsync("/api/admin/import",
            new { csvContent = csv, columnMapping = Mapping }, adminToken);
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var secondResult = await second.Content.ReadFromJsonAsync<RosterImportResultDto>();
        Assert.That(secondResult!.Imported, Is.EqualTo(0));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var count = db.Users.Count(u => u.Email == newEmail);
        Assert.That(count, Is.EqualTo(1));

        var audited = db.AuditEntries.Any(a => a.Action == "RosterImported");
        Assert.That(audited, Is.True);
    }

    private sealed record RosterPreviewDto(List<RosterPreviewRowDto> Rows, int Ready, int Duplicates, int NeedsConsent);
    private sealed record RosterPreviewRowDto(int Line, string Name, string Email, string Cohort, string Status);
    private sealed record RosterImportResultDto(int Imported, int SkippedDuplicates, int NeedsConsent);
}

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

    private async Task<SeededImportFixture> SeedImportFixtureAsync()
    {
        var response = await Client.PostAsync("/api/_test/youth-curriculum", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SeededImportFixture>())!;
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

    [Test]
    [Property("Req", "L2-REPORT-004")]
    public async Task Preview_and_commit_classify_invalid_and_in_file_duplicate_rows_consistently()
    {
        // Traces to: L2-REPORT-004 AC2..5, L2-NFR-001, L2-CONSENT-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var fixture = await SeedImportFixtureAsync();
        var readyEmail = $"import.valid.{Guid.NewGuid():N}@example.com";
        var consentEmail = $"import.consent.{Guid.NewGuid():N}@example.com";
        var csv =
            "name,email,consent,cohort,mentor,track,ignored\n" +
            $"Alice Ready,{readyEmail},yes,{fixture.CohortName},{fixture.MentorName},{fixture.PrimaryTrackName},=not-imported\n" +
            $"Alice Again,{readyEmail},yes,{fixture.CohortName},{fixture.MentorName},{fixture.PrimaryTrackName},ignored\n" +
            $"Broken Email,not-an-email,yes,{fixture.CohortName},{fixture.MentorName},{fixture.PrimaryTrackName},ignored\n" +
            $"Consent Follow-up,{consentEmail},no,{fixture.CohortName},{fixture.MentorName},{fixture.PrimaryTrackName},ignored\n";
        var mapping = new Dictionary<string, string>(Mapping)
        {
            ["cohort"] = "Cohort",
            ["mentor"] = "Mentor",
            ["track"] = "Track",
            ["ignored"] = "Skip",
        };

        var previewResponse = await PostJsonAsync(
            "/api/admin/import/preview",
            new { csvContent = csv, columnMapping = mapping },
            adminToken);
        Assert.That(previewResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var preview = await previewResponse.Content.ReadFromJsonAsync<RosterPreviewDto>();
        var readyRow = preview!.Rows.Single(item => item.Email == readyEmail && item.Status == "Ready");
        Assert.Multiple(() =>
        {
            Assert.That(preview.Ready, Is.EqualTo(1));
            Assert.That(preview.Duplicates, Is.EqualTo(1));
            Assert.That(preview.NeedsConsent, Is.EqualTo(1));
            Assert.That(preview.Invalid, Is.EqualTo(1));
            Assert.That(preview.Rows.Single(item => item.Email == "not-an-email").Status, Is.EqualTo("Invalid"));
            Assert.That(readyRow.Mentor, Is.EqualTo(fixture.MentorName));
            Assert.That(readyRow.Track, Is.EqualTo(fixture.PrimaryTrackName));
        });

        var commitResponse = await PostJsonAsync(
            "/api/admin/import",
            new { csvContent = csv, columnMapping = mapping },
            adminToken);
        var result = await commitResponse.Content.ReadFromJsonAsync<RosterImportResultDto>();
        Assert.Multiple(() =>
        {
            Assert.That(result!.Imported, Is.EqualTo(1));
            Assert.That(result.SkippedDuplicates, Is.EqualTo(1));
            Assert.That(result.NeedsConsent, Is.EqualTo(1));
            Assert.That(result.Invalid, Is.EqualTo(1));
        });

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(db.Users.Count(item => item.Email == readyEmail), Is.EqualTo(1));
        Assert.That(db.Users.Any(item => item.Email == "not-an-email"), Is.False);
        var imported = db.Users.Single(item => item.Email == readyEmail);
        Assert.Multiple(() =>
        {
            Assert.That(imported.CohortId, Is.EqualTo(fixture.CohortId));
            Assert.That(imported.MentorId, Is.EqualTo(fixture.MentorId));
            Assert.That(db.YouthTrackEnrollments.Any(item =>
                item.YouthUserId == imported.Id && item.TrackId == fixture.PrimaryTrackId), Is.True);
        });
    }

    [Test]
    [Property("Req", "L2-REPORT-004")]
    public async Task Import_rejects_oversized_content_invalid_mappings_and_non_admin_callers()
    {
        // Traces to: L2-REPORT-004 AC1..2, L2-NFR-001, L2-AUTH-010
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var csv = "name,email\nAlice,alice@example.com\n";

        var noEmail = await PostJsonAsync(
            "/api/admin/import/preview",
            new { csvContent = csv, columnMapping = new Dictionary<string, string> { ["name"] = "Name" } },
            adminToken);
        Assert.That(noEmail.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var unknownField = await PostJsonAsync(
            "/api/admin/import/preview",
            new { csvContent = csv, columnMapping = new Dictionary<string, string> { ["email"] = "Password" } },
            adminToken);
        Assert.That(unknownField.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var oversized = "name,email\n" + new string('a', 2 * 1024 * 1024) + ",alice@example.com\n";
        var oversizedResponse = await PostJsonAsync(
            "/api/admin/import/preview",
            new { csvContent = oversized, columnMapping = Mapping },
            adminToken);
        Assert.That(oversizedResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var denied = await PostJsonAsync(
            "/api/admin/import/preview",
            new { csvContent = csv, columnMapping = Mapping },
            youthToken);
        Assert.That(denied.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private sealed record RosterPreviewDto(
        List<RosterPreviewRowDto> Rows,
        int Ready,
        int Duplicates,
        int NeedsConsent,
        int Invalid);
    private sealed record RosterPreviewRowDto(
        int Line,
        string Name,
        string Email,
        string Cohort,
        string Mentor,
        string Track,
        string Status,
        string? Message);
    private sealed record RosterImportResultDto(int Imported, int SkippedDuplicates, int NeedsConsent, int Invalid);
    private sealed record SeededImportFixture(
        Guid CohortId,
        string CohortName,
        Guid MentorId,
        string MentorName,
        Guid PrimaryTrackId,
        string PrimaryTrackName);
}

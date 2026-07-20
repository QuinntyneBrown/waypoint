// Acceptance Test
// Traces to: L2-REPORT-003 (CSV export of a cohort report + audit-logged; audit-log export is admin-only)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Reporting;

[TestFixture]
[Property("Req", "L2-REPORT-003")]
public sealed class ExportTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string? token = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (token is not null)
        {
            Authorize(request, token);
        }
        return await Client.SendAsync(request);
    }

    private async Task<Guid> SeedYouthAsync(Guid cohortId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = $"seed.youth.{Guid.NewGuid():N}@example.com",
            DisplayName = "Seed Youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CohortId = cohortId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    private async Task<SeededReportFixture> SeedReportFixtureAsync()
    {
        var response = await Client.PostAsync("/api/_test/youth-curriculum", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SeededReportFixture>())!;
    }

    private async Task<string> SignInAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResultDto>())!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-REPORT-003")]
    public async Task Admin_exports_a_cohort_report_csv_and_it_is_audit_logged()
    {
        // Traces to: L2-REPORT-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var cohortId = Guid.NewGuid();
        var youthId = await SeedYouthAsync(cohortId);

        var response = await SendAsync(HttpMethod.Get, $"/api/reports/cohort/{cohortId}/export.csv", token: adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var csv = await response.Content.ReadFromJsonAsync<CsvFileDto>();
        Assert.That(csv!.Content, Does.Contain("LessonsCompleted")); // header row
        Assert.That(csv.Content, Does.Contain(youthId.ToString()));  // a data row

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logged = db.AuditEntries.Any(a => a.Action == "DataExported" && a.TargetType == "Cohort" && a.TargetId == cohortId);
        Assert.That(logged, Is.True);
    }

    [Test]
    [Property("Req", "L2-REPORT-003")]
    public async Task The_audit_log_csv_export_is_admin_only()
    {
        // Traces to: L2-REPORT-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var youthResponse = await SendAsync(HttpMethod.Get, "/api/admin/audit/export.csv", token: youthToken);
        Assert.That(youthResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var adminResponse = await SendAsync(HttpMethod.Get, "/api/admin/audit/export.csv", token: adminToken);
        Assert.That(adminResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var csv = await adminResponse.Content.ReadFromJsonAsync<CsvFileDto>();
        Assert.That(csv!.Content, Does.Contain("Action")); // header row present
    }

    [Test]
    [Property("Req", "L2-REPORT-003")]
    public async Task Admin_exports_each_filtered_standard_report_as_real_csv_or_pdf_files()
    {
        // Traces to: L2-REPORT-001 AC1..AC4; L2-REPORT-003 AC1..AC3; L2-CONSENT-003
        var included = await SeedReportFixtureAsync();
        var excluded = await SeedReportFixtureAsync();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var seededYouth = await db.Users.FindAsync(included.YouthId);
            seededYouth!.DisplayName = "=2+3";
            await db.SaveChangesAsync();
        }
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var from = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var filter = $"from={from}&to={to}&cohortId={included.CohortId}&trackId={included.PrimaryTrackId}&mentorId={included.MentorId}";

        var overview = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/export?report=overview&format=csv&{filter}",
            adminToken);
        Assert.Multiple(() =>
        {
            Assert.That(overview.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(overview.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/csv"));
            Assert.That(overview.Content.Headers.ContentDisposition?.FileNameStar, Does.Match("overview-report.*\\.csv"));
        });
        var overviewCsv = await overview.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(overviewCsv, Does.Contain(included.CohortName));
            Assert.That(overviewCsv, Does.Contain(included.MentorName));
            Assert.That(overviewCsv, Does.Not.Contain(excluded.CohortName));
        });

        var youthResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/export?report=youth&format=csv&youthId={included.YouthId}&{filter}",
            adminToken);
        Assert.That(youthResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var youthCsv = await youthResponse.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(youthCsv, Does.Contain("'=2+3"), "formula-like values must be neutralized");
            Assert.That(youthCsv, Does.Contain(included.ProgressQuizTitle));
            Assert.That(youthCsv, Does.Contain(included.ProgressCompetencyName));
        });

        var cohort = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/export?report=cohort&format=pdf&cohortId={included.CohortId}&compareCohortId={excluded.CohortId}&{filter}",
            adminToken);
        Assert.That(cohort.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var cohortPdf = await cohort.Content.ReadAsByteArrayAsync();
        Assert.That(System.Text.Encoding.ASCII.GetString(cohortPdf, 0, 8), Is.EqualTo("%PDF-1.4"));

        var staff = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/export?report=staff&format=csv&{filter}",
            adminToken);
        Assert.That(staff.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await staff.Content.ReadAsStringAsync(), Does.Contain(included.MentorName));

        using var auditScope = Factory.Services.CreateScope();
        var auditDb = auditScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var exports = auditDb.AuditEntries
            .Where(item => item.Action == "DataExported" && item.TargetType == "Report")
            .ToList();
        Assert.Multiple(() =>
        {
            Assert.That(exports, Has.Count.EqualTo(4));
            Assert.That(exports.Any(item => item.Details!.Contains($"cohortId={included.CohortId}")), Is.True);
            Assert.That(exports.Select(item => item.Details), Has.Some.Contains("report=overview;format=csv"));
            Assert.That(exports.Select(item => item.Details), Has.Some.Contains("report=cohort;format=pdf"));
        });
    }

    [Test]
    [Property("Req", "L2-REPORT-003")]
    public async Task Mentor_exports_are_resource_scoped_and_staff_exports_are_admin_only()
    {
        // Traces to: L2-REPORT-003 AC3, L2-AUTH-010
        var mine = await SeedReportFixtureAsync();
        var other = await SeedReportFixtureAsync();
        var mentorToken = await SignInAsync(mine.MentorEmail, mine.MentorPassword);

        var ownYouth = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/export?report=youth&format=pdf&youthId={mine.YouthId}",
            mentorToken);
        Assert.That(ownYouth.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var otherYouth = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/export?report=youth&format=csv&youthId={other.YouthId}",
            mentorToken);
        Assert.That(otherYouth.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var otherCohort = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/export?report=cohort&format=csv&cohortId={other.CohortId}",
            mentorToken);
        Assert.That(otherCohort.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var legacyOtherCohort = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/cohort/{other.CohortId}/export.csv",
            mentorToken);
        Assert.That(legacyOtherCohort.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var staff = await SendAsync(
            HttpMethod.Get,
            "/api/reports/export?report=staff&format=csv",
            mentorToken);
        Assert.That(staff.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-REPORT-003")]
    public async Task Audit_exports_apply_filters_in_csv_and_pdf_and_remain_admin_only()
    {
        // Traces to: L2-REPORT-003 AC1..AC3, L2-AUTH-010, L2-CONSENT-003
        var actor = Guid.NewGuid();
        var includedId = Guid.NewGuid();
        var excludedId = Guid.NewGuid();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AuditEntries.AddRange(
                new AuditEntry
                {
                    ActorUserId = actor,
                    Action = "DataExported",
                    TargetType = "Report",
                    TargetId = includedId,
                    At = DateTime.UtcNow.AddHours(-1),
                },
                new AuditEntry
                {
                    ActorUserId = actor,
                    Action = "AccountDeactivated",
                    TargetType = "User",
                    TargetId = excludedId,
                    At = DateTime.UtcNow.AddHours(-1),
                });
            await db.SaveChangesAsync();
        }
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var from = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var url = $"/api/admin/audit/export?format=csv&action=DataExported&actorUserId={actor}&from={from}&to={to}";

        var denied = await SendAsync(HttpMethod.Get, url, youthToken);
        Assert.That(denied.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var csvResponse = await SendAsync(HttpMethod.Get, url, adminToken);
        Assert.That(csvResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var csv = await csvResponse.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(csv, Does.Contain(includedId.ToString()));
            Assert.That(csv, Does.Not.Contain(excludedId.ToString()));
            Assert.That(csvResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/csv"));
        });

        var pdfResponse = await SendAsync(
            HttpMethod.Get,
            url.Replace("format=csv", "format=pdf"),
            adminToken);
        Assert.That(pdfResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var pdf = await pdfResponse.Content.ReadAsByteArrayAsync();
        Assert.That(System.Text.Encoding.ASCII.GetString(pdf, 0, 8), Is.EqualTo("%PDF-1.4"));

        using var auditScope = Factory.Services.CreateScope();
        var auditDb = auditScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logged = auditDb.AuditEntries
            .Where(item => item.Action == "DataExported"
                && item.TargetType == "AuditLog"
                && item.Details != null)
            .ToList();
        Assert.Multiple(() =>
        {
            Assert.That(logged, Has.Count.EqualTo(2));
            Assert.That(logged.Select(item => item.Details), Has.Some.Contains("format=csv"));
            Assert.That(logged.Select(item => item.Details), Has.Some.Contains("format=pdf"));
        });
    }

    private sealed record CsvFileDto(string FileName, string Content);
    private sealed record AuthResultDto(string AccessToken);

    private sealed record SeededReportFixture(
        Guid YouthId,
        Guid CohortId,
        string CohortName,
        Guid PrimaryTrackId,
        Guid MentorId,
        string MentorName,
        string MentorEmail,
        string MentorPassword,
        string ProgressQuizTitle,
        string ProgressCompetencyName);
}

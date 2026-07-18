// Acceptance Test
// Traces to: L2-REPORT-003 (CSV export of a cohort report + audit-logged; audit-log export is admin-only)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Domain;
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

    private sealed record CsvFileDto(string FileName, string Content);
}

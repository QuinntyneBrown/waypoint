using System.Net;
using System.Net.Http.Json;

namespace WordUp.IntegrationTests.Admin;

// Acceptance Test
// Traces to: L2-ADMIN-006 (organization settings)
[TestFixture]
[Property("Req", "L2-ADMIN-006")]
public sealed class OrgSettingsTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body, string? token)
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

    [Test]
    [Property("Req", "L2-ADMIN-006")]
    public async Task Updating_org_settings_persists_and_non_admin_is_forbidden()
    {
        // Traces to: L2-ADMIN-006
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var body = new
        {
            name = "Word Up Community",
            displayName = "WUC",
            logoUrl = "https://example.com/logo.png",
            city = "Abuja",
            contactEmail = "hello@wordup.example",
            consentStatement = "We care about consent.",
            requireReConsentEachTerm = false,
            requireMediaConsent = true,
        };

        var update = await SendAsync(HttpMethod.Put, "/api/org-settings", body, adminToken);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var get = await SendAsync(HttpMethod.Get, "/api/org-settings", null, adminToken);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var settings = await get.Content.ReadFromJsonAsync<OrgSettingsDto>();
        Assert.That(settings!.Name, Is.EqualTo("Word Up Community"));
        Assert.That(settings.City, Is.EqualTo("Abuja"));
        Assert.That(settings.RequireMediaConsent, Is.True);
        Assert.That(settings.RequireReConsentEachTerm, Is.False);

        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await SendAsync(HttpMethod.Put, "/api/org-settings", body, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private sealed record OrgSettingsDto(
        string Name,
        string? DisplayName,
        string? LogoUrl,
        string? City,
        string? ContactEmail,
        string? ConsentStatement,
        bool RequireReConsentEachTerm,
        bool RequireMediaConsent);
}

// Acceptance Test
// Traces to: L2-PUB-003 (contact/volunteer/donation captured + acknowledged; screening-consent gate),
//            L2-NFR-001 (all public free-text sanitized server-side against stored XSS)
using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Common;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Public;

[TestFixture]
[Property("Req", "L2-PUB-003")]
public sealed class PublicFormsTests : IntegrationTestBase
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

    // Traces to: L2-PUB-003 (AC1 contact captured + acknowledged anonymously)
    [Test]
    [Property("Req", "L2-PUB-003")]
    public async Task Contact_submission_is_accepted_anonymously_acknowledged_and_persisted()
    {
        var response = await SendAsync(HttpMethod.Post, "/api/public/contact", body: new
        {
            name = "Jordan Visitor",
            email = "jordan@example.com",
            topic = "Partnership",
            message = "We'd love to partner with your program.",
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<ContactResultDto>();
        Assert.That(result!.ContactRequestId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Acknowledgement, Is.Not.Empty);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await db.ContactRequests.FirstOrDefaultAsync(c => c.Id == result.ContactRequestId);
        Assert.That(row, Is.Not.Null);
        Assert.That(row!.Topic, Is.EqualTo("Partnership"));
        Assert.That(row.Email, Is.EqualTo("jordan@example.com"));
        Assert.That(row.OwnerUserId, Is.Null);
    }

    [Test]
    public async Task Authenticated_public_submission_records_explicit_account_ownership()
    {
        var token = await CreateUserAndGetTokenAsync(Roles.Youth);
        var userId = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(token).Subject);

        var response = await SendAsync(HttpMethod.Post, "/api/public/contact", body: new
        {
            name = "Authenticated author",
            email = "shared-or-alternate@example.com",
            topic = "Ownership",
            message = "This record is linked by authentication, not by its submitted email.",
        }, token: token);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ContactResultDto>();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await db.ContactRequests.FindAsync(result!.ContactRequestId);
        Assert.That(row!.OwnerUserId, Is.EqualTo(userId));
    }

    // Traces to: L2-PUB-003 (AC1 malformed/missing input blocked with field-level errors)
    [Test]
    [Property("Req", "L2-PUB-003")]
    public async Task Contact_submission_with_missing_fields_or_malformed_email_is_rejected()
    {
        var malformedEmail = await SendAsync(HttpMethod.Post, "/api/public/contact", body: new
        {
            name = "Jordan Visitor",
            email = "not-an-email",
            topic = "Partnership",
            message = "Hello.",
        });
        Assert.That(malformedEmail.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var missingMessage = await SendAsync(HttpMethod.Post, "/api/public/contact", body: new
        {
            name = "Jordan Visitor",
            email = "jordan@example.com",
            topic = "Partnership",
            message = "",
        });
        Assert.That(missingMessage.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // Traces to: L2-PUB-003 (AC2 volunteer submission blocked unless screening-consent is acknowledged)
    [Test]
    [Property("Req", "L2-PUB-003")]
    public async Task Volunteer_signup_is_blocked_without_screening_consent_and_accepted_with_it()
    {
        var withoutConsent = await SendAsync(HttpMethod.Post, "/api/public/volunteer-signup", body: new
        {
            name = "Sam Helper",
            email = "sam@example.com",
            phone = "555-0100",
            areasOfInterest = "Mentoring",
            screeningConsentAck = false,
        });
        Assert.That(withoutConsent.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var withConsent = await SendAsync(HttpMethod.Post, "/api/public/volunteer-signup", body: new
        {
            name = "Sam Helper",
            email = "sam@example.com",
            phone = "555-0100",
            areasOfInterest = "Mentoring",
            screeningConsentAck = true,
        });
        Assert.That(withConsent.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await withConsent.Content.ReadFromJsonAsync<VolunteerResultDto>();
        Assert.That(result!.VolunteerSignupId, Is.Not.EqualTo(Guid.Empty));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await db.VolunteerSignups.FirstOrDefaultAsync(v => v.Id == result.VolunteerSignupId);
        Assert.That(row, Is.Not.Null);
        Assert.That(row!.ScreeningConsentAck, Is.True);
    }

    // Traces to: L2-PUB-003 (AC4 sanitize free-text) + L2-NFR-001 (stored-XSS neutralised server-side)
    [Test]
    [Property("Req", "L2-PUB-003")]
    [Property("Req", "L2-NFR-001")]
    public async Task Contact_message_with_a_script_payload_is_stored_html_encoded()
    {
        const string payload = "<script>alert('xss')</script>";

        var response = await SendAsync(HttpMethod.Post, "/api/public/contact", body: new
        {
            name = "Mallory",
            email = "mallory@example.com",
            topic = "Feedback",
            message = payload,
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<ContactResultDto>();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await db.ContactRequests.FirstOrDefaultAsync(c => c.Id == result!.ContactRequestId);
        Assert.That(row, Is.Not.Null);

        // The raw executable markup must not survive to storage; it is HTML-encoded (L2-NFR-001).
        Assert.That(row!.Message, Does.Not.Contain("<script>"));
        Assert.That(row.Message, Does.Contain("&lt;script&gt;"));
    }

    private sealed record ContactResultDto(Guid ContactRequestId, string Acknowledgement);

    private sealed record VolunteerResultDto(Guid VolunteerSignupId, string Acknowledgement);
}

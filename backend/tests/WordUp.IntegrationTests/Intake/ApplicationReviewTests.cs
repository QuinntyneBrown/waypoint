// Acceptance Test
// Traces to: L2-INTAKE-002 (admin review pipeline — list submitted applications for review; admin-only)
using System.Net;
using System.Net.Http.Json;

namespace WordUp.IntegrationTests.Intake;

[TestFixture]
[Property("Req", "L2-INTAKE-002")]
public sealed class ApplicationReviewTests : IntegrationTestBase
{
    private static object ValidMinorSubmission() => new
    {
        firstName = "Ada",
        lastName = "Okafor",
        age = 15,
        whoIsFilling = "guardian",
        neighbourhood = "Peckham",
        cohortInterest = "Peckham Autumn cohort",
        whyInterested = "I want to learn to write and perform.",
        trackInterest = "Spoken Word",
        guardianName = "Ngozi Okafor",
        guardianEmail = "guardian@example.com",
        guardianPhone = "07123456789",
        participationConsentAck = true,
        privacyAck = true,
    };

    private async Task<HttpResponseMessage> GetAsync(string url, string? accessToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (accessToken is not null)
        {
            Authorize(request, accessToken);
        }
        return await Client.SendAsync(request);
    }

    // Traces to: L2-INTAKE-002 (AC1 admin reviews submitted applications; AC5 admin-only)
    [Test]
    [Property("Req", "L2-INTAKE-002")]
    public async Task Admin_lists_submitted_applications_and_a_non_admin_is_forbidden()
    {
        // A member of the public submits an application (anonymous).
        var submit = await Client.PostAsJsonAsync("/api/applications", ValidMinorSubmission());
        Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var applicationId = (await submit.Content.ReadFromJsonAsync<SubmitResultDto>())!.ApplicationId;

        // AC5: a non-admin cannot list applications for review.
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await GetAsync("/api/applications", youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        // AC1: an admin sees the submitted application, with its review status and minor flag.
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var list = await GetAsync("/api/applications", adminToken);
        Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var items = await list.Content.ReadFromJsonAsync<List<ApplicationSummaryDto>>();
        var item = items!.FirstOrDefault(a => a.Id == applicationId);
        Assert.That(item, Is.Not.Null, "the submitted application should appear in the admin list");
        Assert.That(item!.FirstName, Is.EqualTo("Ada"));
        Assert.That(item.Status, Is.EqualTo("Needs consent"));
        Assert.That(item.IsMinor, Is.True);
    }

    private sealed record SubmitResultDto(Guid ApplicationId);

    private sealed record ApplicationSummaryDto(
        Guid Id,
        string FirstName,
        string LastName,
        int Age,
        bool IsMinor,
        string Neighbourhood,
        string? TrackInterest,
        string Status,
        DateTime CreatedAt);
}

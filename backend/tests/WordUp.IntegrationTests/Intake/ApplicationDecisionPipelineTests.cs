using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Intake;

// Acceptance Tests
// Traces to: L2-INTAKE-002
[TestFixture]
[Property("Req", "L2-INTAKE-002")]
public sealed class ApplicationDecisionPipelineTests : IntegrationTestBase
{
    [Test]
    public async Task Minor_detail_shows_needs_consent_and_full_approval_is_blocked()
    {
        var placement = await SeedPlacementAsync();
        var applicationId = await SubmitAsync(age: 15);
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var detailResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/applications/{applicationId}",
            null,
            adminToken);
        Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await detailResponse.Content.ReadFromJsonAsync<ApplicationDetailDto>();
        Assert.Multiple(() =>
        {
            Assert.That(detail!.Status, Is.EqualTo("Needs consent"));
            Assert.That(detail.ConsentState, Is.EqualTo("Needs consent"));
            Assert.That(detail.CanApprove, Is.False);
            Assert.That(detail.GuardianEmail, Is.EqualTo("guardian.pipeline@example.com"));
            Assert.That(detail.WhyInterested, Does.Contain("leadership"));
        });

        var optionsResponse = await SendAsync(
            HttpMethod.Get,
            "/api/applications/options",
            null,
            adminToken);
        Assert.That(optionsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var options = await optionsResponse.Content.ReadFromJsonAsync<ApplicationOptionsDto>();
        Assert.That(options!.Cohorts.Single(item => item.Id == placement.CohortId).Name, Is.Not.Empty);
        Assert.That(options.Mentors.Single(item => item.Id == placement.MentorId).CohortIds,
            Does.Contain(placement.CohortId));

        var filtered = await SendAsync(
            HttpMethod.Get,
            "/api/applications?status=Needs%20consent",
            null,
            adminToken);
        var rows = await filtered.Content.ReadFromJsonAsync<List<ApplicationSummaryDto>>();
        Assert.That(rows!.Single(item => item.Id == applicationId).Status, Is.EqualTo("Needs consent"));

        var approval = await SendAsync(
            HttpMethod.Post,
            $"/api/applications/{applicationId}/approve",
            new
            {
                cohortId = placement.CohortId,
                mentorId = placement.MentorId,
                trackInterest = placement.TrackName,
                decisionNote = "Strong application.",
            },
            adminToken);
        Assert.That(approval.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(
            await db.Applications.Where(item => item.Id == applicationId)
                .Select(item => item.CreatedYouthUserId)
                .SingleAsync(),
            Is.Null);
    }

    [Test]
    public async Task Adult_approval_validates_placement_enrols_notifies_and_audits()
    {
        var placement = await SeedPlacementAsync();
        var applicationId = await SubmitAsync(age: 18);
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var invalid = await SendAsync(
            HttpMethod.Post,
            $"/api/applications/{applicationId}/approve",
            new
            {
                cohortId = placement.CohortId,
                mentorId = placement.OtherMentorId,
                trackInterest = placement.TrackName,
                decisionNote = "Invalid pairing.",
            },
            adminToken);
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var approved = await SendAsync(
            HttpMethod.Post,
            $"/api/applications/{applicationId}/approve",
            new
            {
                cohortId = placement.CohortId,
                mentorId = placement.MentorId,
                trackInterest = placement.TrackName,
                decisionNote = "Welcome to the cohort.",
            },
            adminToken);
        Assert.That(approved.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await approved.Content.ReadFromJsonAsync<ApproveResultDto>();
        Assert.That(result!.Status, Is.EqualTo("Approved"));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var youth = await db.Users.SingleAsync(item => item.Id == result.YouthUserId);
        Assert.Multiple(() =>
        {
            Assert.That(youth.Status, Is.EqualTo(UserStatus.Active));
            Assert.That(youth.CohortId, Is.EqualTo(placement.CohortId));
            Assert.That(youth.MentorId, Is.EqualTo(placement.MentorId));
            Assert.That(
                db.YouthTrackEnrollments.Any(item =>
                    item.YouthUserId == youth.Id && item.TrackId == placement.TrackId),
                Is.True);
            Assert.That(
                db.AuditEntries.Any(item =>
                    item.Action == "ApplicationApproved" && item.TargetId == applicationId),
                Is.True);
        });
        var email = scope.ServiceProvider.GetRequiredService<CapturingEmailSender>()
            .LastTo("guardian.pipeline@example.com");
        Assert.That(email, Is.Not.Null);
        Assert.That(email!.Subject, Does.Contain("approved").IgnoreCase);
    }

    [Test]
    public async Task Decline_keeps_the_note_creates_no_account_and_notifies()
    {
        var applicationId = await SubmitAsync(age: 18);
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var response = await SendAsync(
            HttpMethod.Post,
            $"/api/applications/{applicationId}/decline",
            new { decisionNote = "No place is available this term." },
            adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var application = await db.Applications.SingleAsync(item => item.Id == applicationId);
        Assert.Multiple(() =>
        {
            Assert.That(application.Status, Is.EqualTo(ApplicationStatus.Declined));
            Assert.That(application.DecisionNote, Is.EqualTo("No place is available this term."));
            Assert.That(application.CreatedYouthUserId, Is.Null);
            Assert.That(
                db.AuditEntries.Any(item =>
                    item.Action == "ApplicationDeclined" && item.TargetId == applicationId),
                Is.True);
        });
        Assert.That(
            scope.ServiceProvider.GetRequiredService<CapturingEmailSender>()
                .LastTo("guardian.pipeline@example.com"),
            Is.Not.Null);
    }

    private async Task<Placement> SeedPlacementAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var track = new Track
        {
            Name = $"Leadership {Guid.NewGuid():N}",
            Key = $"leadership-{Guid.NewGuid():N}",
            Status = PublishStatus.Published,
            Enabled = true,
        };
        var mentor = new User
        {
            Email = $"mentor.pipeline.{Guid.NewGuid():N}@example.com",
            DisplayName = "Mentor Pipeline",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        var otherMentor = new User
        {
            Email = $"mentor.other.{Guid.NewGuid():N}@example.com",
            DisplayName = "Other Mentor",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        var cohort = new Cohort
        {
            Name = $"Pipeline Cohort {Guid.NewGuid():N}",
            TrackId = track.Id,
            MentorId = mentor.Id,
            TermStart = DateTime.UtcNow.Date.AddMonths(-1),
            TermEnd = DateTime.UtcNow.Date.AddMonths(6),
        };
        db.AddRange(track, mentor, otherMentor, cohort);
        await db.SaveChangesAsync();
        return new Placement(
            cohort.Id,
            mentor.Id,
            otherMentor.Id,
            track.Id,
            track.Name);
    }

    private async Task<Guid> SubmitAsync(int age)
    {
        var response = await Client.PostAsJsonAsync("/api/applications", new
        {
            firstName = "Amina",
            lastName = $"Pipeline-{Guid.NewGuid():N}",
            age,
            whoIsFilling = age < 18 ? "guardian" : "self",
            neighbourhood = "Rexdale",
            cohortInterest = "Saturday cohort",
            whyInterested = "I want to grow in leadership.",
            trackInterest = "Leadership",
            guardianName = "Pipeline Guardian",
            guardianEmail = "guardian.pipeline@example.com",
            guardianPhone = "4165550199",
            participationConsentAck = true,
            privacyAck = true,
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SubmitResultDto>())!.ApplicationId;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body,
        string token)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private sealed record Placement(
        Guid CohortId,
        Guid MentorId,
        Guid OtherMentorId,
        Guid TrackId,
        string TrackName);
    private sealed record SubmitResultDto(Guid ApplicationId);
    private sealed record ApproveResultDto(Guid YouthUserId, string Status);
    private sealed record ApplicationSummaryDto(Guid Id, string Status);
    private sealed record ApplicationDetailDto(
        Guid Id,
        string Status,
        string ConsentState,
        bool CanApprove,
        string? GuardianEmail,
        string WhyInterested);
    private sealed record ApplicationOptionsDto(
        List<OptionDto> Cohorts,
        List<MentorOptionDto> Mentors,
        List<OptionDto> Tracks);
    private sealed record OptionDto(Guid Id, string Name);
    private sealed record MentorOptionDto(Guid Id, string Name, List<Guid> CohortIds);
}

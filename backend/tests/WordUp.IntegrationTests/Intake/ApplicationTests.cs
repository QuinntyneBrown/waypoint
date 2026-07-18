using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Intake;

// Acceptance Test
// Traces to: L2-INTAKE-001 (public application submission),
//            L2-INTAKE-002 (review: approve provisions youth, decline creates no account)
[TestFixture]
[Property("Req", "L2-INTAKE-001")]
public sealed class ApplicationTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string? accessToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        if (accessToken is not null)
        {
            Authorize(request, accessToken);
        }
        return await Client.SendAsync(request);
    }

    private static object ValidMinorSubmission(string? guardianEmail = "guardian@example.com", int age = 15) => new
    {
        firstName = "Ada",
        lastName = "Okafor",
        age,
        neighbourhood = "Peckham",
        whoIsFilling = "guardian",
        cohortInterest = "Peckham Autumn cohort",
        whyInterested = "I want to learn to write and perform.",
        trackInterest = "Spoken Word",
        guardianName = "Ngozi Okafor",
        guardianEmail,
        guardianPhone = "07123456789",
        participationConsentAck = true,
        privacyAck = true,
    };

    private async Task<int> UserCountAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Users.CountAsync();
    }

    // Traces to: L2-INTAKE-001 (AC1, AC2, AC4)
    [Test]
    [Property("Req", "L2-INTAKE-001")]
    public async Task Valid_submission_succeeds_and_creates_no_user_account()
    {
        var before = await UserCountAsync();

        var response = await PostAsync("/api/applications", ValidMinorSubmission());

        Assert.That((int)response.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        var result = await response.Content.ReadFromJsonAsync<SubmitResultDto>();
        Assert.That(result!.ApplicationId, Is.Not.EqualTo(Guid.Empty));

        // AC4: no account is provisioned at submission time.
        Assert.That(await UserCountAsync(), Is.EqualTo(before));
    }

    // Traces to: L2-INTAKE-001 (form ownership and cohort of interest are captured)
    [Test]
    [Property("Req", "L2-INTAKE-001")]
    public async Task Submission_requires_and_records_who_is_filling_and_cohort_interest()
    {
        var missing = await PostAsync("/api/applications", new
        {
            firstName = "Ada",
            lastName = "Okafor",
            age = 18,
            neighbourhood = "Peckham",
            whyInterested = "I want to grow as a leader.",
            participationConsentAck = true,
            privacyAck = true,
        });
        Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var response = await PostAsync("/api/applications", ValidMinorSubmission());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var id = (await response.Content.ReadFromJsonAsync<SubmitResultDto>())!.ApplicationId;

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var recorded = await db.Applications
            .Where(a => a.Id == id)
            .Select(a => new
            {
                WhoIsFilling = EF.Property<string>(a, "WhoIsFilling"),
                CohortInterest = EF.Property<string>(a, "CohortInterest"),
            })
            .SingleAsync();
        Assert.That(recorded.WhoIsFilling, Is.EqualTo("guardian"));
        Assert.That(recorded.CohortInterest, Is.EqualTo("Peckham Autumn cohort"));
    }

    // Traces to: L2-INTAKE-001 (AC3 age range, AC2 guardian details)
    [Test]
    [Property("Req", "L2-INTAKE-001")]
    public async Task Age_outside_range_and_minor_missing_guardian_email_are_rejected()
    {
        // AC3: an over-age applicant is rejected.
        var tooOld = await PostAsync("/api/applications", ValidMinorSubmission(age: 20));
        Assert.That(tooOld.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // AC2: a minor without a guardian email is rejected.
        var noGuardian = await PostAsync("/api/applications", ValidMinorSubmission(guardianEmail: null, age: 14));
        Assert.That(noGuardian.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // Traces to: L2-INTAKE-002 (AC3 approve provisions youth, AC5 authorization)
    [Test]
    [Property("Req", "L2-INTAKE-002")]
    public async Task Approving_provisions_a_youth_and_a_non_admin_is_forbidden()
    {
        Guid cohortId;
        Guid mentorId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var mentor = new User
            {
                Email = $"mentor.{Guid.NewGuid():N}@example.com",
                DisplayName = "Mentor",
                Role = Roles.Mentor,
                Status = UserStatus.Active,
            };
            var cohort = new Cohort
            {
                Name = "Autumn 2026",
                TermStart = new DateTime(2026, 9, 1),
                TermEnd = new DateTime(2026, 12, 1),
                MentorId = mentor.Id,
            };
            db.Add(mentor);
            db.Add(cohort);
            await db.SaveChangesAsync();
            cohortId = cohort.Id;
            mentorId = mentor.Id;
        }

        var submit = await PostAsync("/api/applications", ValidMinorSubmission());
        var applicationId = (await submit.Content.ReadFromJsonAsync<SubmitResultDto>())!.ApplicationId;

        var approveBody = new { cohortId, mentorId, trackInterest = "Spoken Word", decisionNote = "Great fit." };

        // AC5: a youth cannot approve applications.
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await PostAsync($"/api/applications/{applicationId}/approve", approveBody, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        // AC3: a program administrator approves and a Youth account is provisioned.
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var approved = await PostAsync($"/api/applications/{applicationId}/approve", approveBody, adminToken);
        Assert.That(approved.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await approved.Content.ReadFromJsonAsync<ApproveResultDto>();
        Assert.That(result!.YouthUserId, Is.Not.EqualTo(Guid.Empty));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var youth = await db.Users.FirstOrDefaultAsync(u => u.Id == result.YouthUserId);
            Assert.That(youth, Is.Not.Null);
            Assert.That(youth!.Role, Is.EqualTo(Roles.Youth));
            Assert.That(youth.Status, Is.EqualTo(UserStatus.Active));
            Assert.That(youth.CohortId, Is.EqualTo(cohortId));
            Assert.That(youth.MentorId, Is.EqualTo(mentorId));

            var app = await db.Applications.FirstAsync(a => a.Id == applicationId);
            Assert.That(app.Status, Is.EqualTo(ApplicationStatus.Approved));
            Assert.That(app.CreatedYouthUserId, Is.EqualTo(result.YouthUserId));
        }
    }

    // Traces to: L2-INTAKE-002 (AC4 decline creates no account)
    [Test]
    [Property("Req", "L2-INTAKE-002")]
    public async Task Declining_marks_the_application_declined_and_creates_no_account()
    {
        var submit = await PostAsync("/api/applications", ValidMinorSubmission());
        var applicationId = (await submit.Content.ReadFromJsonAsync<SubmitResultDto>())!.ApplicationId;

        var before = await UserCountAsync();

        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var declined = await PostAsync(
            $"/api/applications/{applicationId}/decline", new { decisionNote = "Not eligible this term." }, adminToken);
        Assert.That((int)declined.StatusCode, Is.EqualTo((int)HttpStatusCode.NoContent));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var app = await db.Applications.FirstAsync(a => a.Id == applicationId);
            Assert.That(app.Status, Is.EqualTo(ApplicationStatus.Declined));
            Assert.That(app.CreatedYouthUserId, Is.Null);
        }

        // AC4: declining provisions no account (only the admin we just signed in was added).
        Assert.That(await UserCountAsync(), Is.EqualTo(before + 1));
    }

    private sealed record SubmitResultDto(Guid ApplicationId);
    private sealed record ApproveResultDto(Guid YouthUserId);
}

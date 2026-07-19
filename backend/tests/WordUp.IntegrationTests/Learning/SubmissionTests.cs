using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-006 (assignment submission), L2-LEARN-007 (submission status),
//            L2-LEARN-008 (coding-project submit)
[TestFixture]
[Property("Req", "L2-LEARN-006")]
public sealed class SubmissionTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<(Guid youthId, string token)> SeedYouthAsync()
    {
        var email = $"youth.{Guid.NewGuid():N}@example.com";
        Guid youthId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var youth = new User
            {
                Email = email,
                DisplayName = "Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            db.Users.Add(youth);
            await db.SaveChangesAsync();
            youthId = youth.Id;
        }

        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<WordUp.Application.Authentication.Auth.AuthResult>();
        return (youthId, auth!.AccessToken);
    }

    private async Task<(Guid Id, string Email, string Token)> SeedAssignedMentorAsync(Guid youthId)
    {
        var email = $"mentor.{Guid.NewGuid():N}@example.com";
        Guid mentorId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var mentor = new User
            {
                Email = email,
                DisplayName = "Assigned mentor",
                Role = Roles.Mentor,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            var youth = await db.Users.FindAsync(youthId) ?? throw new InvalidOperationException();
            db.Users.Add(mentor);
            youth.MentorId = mentor.Id;
            await db.SaveChangesAsync();
            mentorId = mentor.Id;
        }

        var signIn = await Client.PostAsJsonAsync(
            "/api/auth/sign-in", new { email, password = "Passw0rd!" });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<WordUp.Application.Authentication.Auth.AuthResult>();
        return (mentorId, email, auth!.AccessToken);
    }

    private async Task<(Guid youthId, string youthToken, string guardianToken)> SeedConsentedMinorAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var guardianEmail = $"guardian.{suffix}@example.com";
        var youthEmail = $"minor.{suffix}@example.com";
        Guid youthId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var guardian = new User
            {
                Email = guardianEmail,
                DisplayName = "Guardian",
                Role = Roles.Guardian,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            var youth = new User
            {
                Email = youthEmail,
                DisplayName = "Minor youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
                GuardianUserId = guardian.Id,
            };
            db.Users.AddRange(guardian, youth);
            db.GuardianConsents.Add(new GuardianConsent
            {
                YouthUserId = youth.Id,
                GuardianUserId = guardian.Id,
                GuardianName = guardian.DisplayName,
                Relationship = "Parent",
                ParticipationConsent = true,
                MediaConsent = true,
                MediaGroupOnly = true,
                Signature = guardian.DisplayName,
                ReferenceId = $"MEDIA-{suffix[..8].ToUpperInvariant()}",
                SignedAt = DateTime.UtcNow.AddDays(-1),
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidUntil = DateTime.UtcNow.AddMonths(6),
            });
            await db.SaveChangesAsync();
            youthId = youth.Id;
        }

        var youthSignIn = await Client.PostAsJsonAsync(
            "/api/auth/sign-in", new { email = youthEmail, password = "Passw0rd!" });
        var guardianSignIn = await Client.PostAsJsonAsync(
            "/api/auth/sign-in", new { email = guardianEmail, password = "Passw0rd!" });
        var youthAuth = await youthSignIn.Content.ReadFromJsonAsync<WordUp.Application.Authentication.Auth.AuthResult>();
        var guardianAuth = await guardianSignIn.Content.ReadFromJsonAsync<WordUp.Application.Authentication.Auth.AuthResult>();
        return (youthId, youthAuth!.AccessToken, guardianAuth!.AccessToken);
    }

    private async Task<HttpResponseMessage> UploadPhotoAsync(Guid assignmentId, string token)
    {
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        var content = new MultipartFormDataContent();
        var image = new ByteArrayContent(png);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(image, "file", "evidence.png");
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/me/assignments/{assignmentId}/uploads")
        { Content = content };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> UploadPdfAsync(
        Guid assignmentId,
        string token,
        string fileName,
        byte[] content)
    {
        var form = new MultipartFormDataContent();
        var document = new ByteArrayContent(content);
        document.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(document, "file", fileName);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/me/assignments/{assignmentId}/uploads")
        { Content = form };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private sealed record SeededAssignment(Guid Id, Guid CriterionId);

    private async Task<SeededAssignment> SeedAssignmentAsync(string submissionTypes = "text")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var assignment = new Assignment
        {
            Title = "Memory Verse",
            TaskDescription = "Write out this week's verse.",
            SubmissionTypes = submissionTypes,
            DueDate = DateTime.UtcNow.AddDays(5),
            TotalPoints = 10,
        };
        db.Assignments.Add(assignment);
        var criterion = new RubricCriterion
        {
            AssignmentId = assignment.Id,
            Description = "Connect the verse to everyday life",
            Points = 10,
            Order = 1,
        };
        db.RubricCriteria.Add(criterion);
        await db.SaveChangesAsync();
        return new SeededAssignment(assignment.Id, criterion.Id);
    }

    [Test]
    [Property("Req", "L2-LEARN-006")]
    public async Task Assignment_brief_returns_task_due_types_and_rubric_points()
    {
        var (_, youthToken) = await SeedYouthAsync();
        var assignment = await SeedAssignmentAsync("text,photo");

        var response = await GetAsync($"/api/me/assignments/{assignment.Id}", youthToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var brief = await response.Content.ReadFromJsonAsync<AssignmentBriefDto>();
        Assert.Multiple(() =>
        {
            Assert.That(brief!.Title, Is.EqualTo("Memory Verse"));
            Assert.That(brief.TaskDescription, Does.Contain("this week's verse"));
            Assert.That(brief.DueDate, Is.Not.Null);
            Assert.That(brief.AllowedSubmissionTypes, Is.EquivalentTo(new[] { "text", "photo" }));
            Assert.That(brief.Rubric, Has.Count.EqualTo(1));
            Assert.That(brief.Rubric[0].Points, Is.EqualTo(10));
            Assert.That(brief.MaxUploadBytes, Is.EqualTo(10 * 1024 * 1024));
        });
    }

    [Test]
    [Property("Req", "L2-LEARN-006")]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Submitting_without_attestation_is_rejected_and_with_attestation_succeeds()
    {
        // Traces to: L2-LEARN-006; L2-ENGAGE-008; L2-PLATFORM-003 AC2
        var (youthId, youthToken) = await SeedYouthAsync();
        var mentor = await SeedAssignedMentorAsync(youthId);
        using (var preferenceScope = Factory.Services.CreateScope())
        {
            var preferenceDb = preferenceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            preferenceDb.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = mentor.Id,
                EventKey = "approval-needed",
                InApp = false,
                Email = true,
            });
            await preferenceDb.SaveChangesAsync();
        }
        var assignment = await SeedAssignmentAsync();

        // Traces to: L2-LEARN-006 (attestation required)
        var noAttest = await PostAsync(
            $"/api/me/assignments/{assignment.Id}/submissions",
            new { bodyText = "My answer", isCodingProject = false, attest = false },
            youthToken);
        Assert.That(noAttest.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var ok = await PostAsync(
            $"/api/me/assignments/{assignment.Id}/submissions",
            new { bodyText = "My answer", isCodingProject = false, attest = true },
            youthToken);
        Assert.That(ok.StatusCode, Is.EqualTo(HttpStatusCode.OK), await ok.Content.ReadAsStringAsync());

        var created = await ok.Content.ReadFromJsonAsync<SubmissionResultDto>();
        var approvalEmail = Factory.Emails.LastTo(mentor.Email);
        Assert.Multiple(() =>
        {
            Assert.That(approvalEmail, Is.Not.Null, "the assigned mentor receives the review request");
            Assert.That(approvalEmail!.Subject, Is.EqualTo("Assignment ready for review"));
            Assert.That(approvalEmail.Body, Does.Contain($"/mentor/reviews/submissions/{created!.Id}"));
            Assert.That(Factory.Changes.Changes.Any(item =>
                item.Audience == "user"
                && item.AudienceId == mentor.Id
                && item.EventName == "notification"
                && JsonSerializer.Serialize(item.Payload).Contains("approval-needed")), Is.False);
        });
        using (var notificationScope = Factory.Services.CreateScope())
        {
            var notificationDb = notificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(notificationDb.Notifications.Any(item =>
                item.UserId == mentor.Id && item.EventKey == "approval-needed"), Is.False);
        }
        var status = await GetAsync($"/api/me/submissions/{created!.Id}", youthToken);
        Assert.That(status.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var view = await status.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.Multiple(() =>
        {
            Assert.That(view!.Status, Is.EqualTo(SubmissionStatus.Submitted.ToString()));
            Assert.That(view.AssignmentTitle, Is.EqualTo("Memory Verse"));
            Assert.That(view.BodyText, Is.EqualTo("My answer"));
            Assert.That(view.Timeline.Select(item => item.Type), Does.Contain("Submitted"));
            Assert.That(view.Timeline.Select(item => item.Type), Does.Contain("UnderReview"));
        });

        var duplicate = await PostAsync(
            $"/api/me/assignments/{assignment.Id}/submissions",
            new { bodyText = "Edited after submit", isCodingProject = false, attest = true },
            youthToken);
        Assert.That(duplicate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    [Property("Req", "L2-LEARN-006")]
    [Property("Req", "L2-NFR-001")]
    public async Task Photo_only_assignment_requires_a_valid_private_png_or_jpeg_upload()
    {
        var (_, ownerToken) = await SeedYouthAsync();
        var (_, otherToken) = await SeedYouthAsync();
        var assignment = await SeedAssignmentAsync("photo");

        var missing = await PostAsync(
            $"/api/me/assignments/{assignment.Id}/submissions",
            new { isCodingProject = false, attest = true },
            ownerToken);
        Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        using var fake = new MultipartFormDataContent();
        var fakeBytes = new ByteArrayContent("<script>alert(1)</script>"u8.ToArray());
        fakeBytes.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        fake.Add(fakeBytes, "file", "evidence.png");
        var fakeRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/me/assignments/{assignment.Id}/uploads")
        { Content = fake };
        Authorize(fakeRequest, ownerToken);
        var rejected = await Client.SendAsync(fakeRequest);
        Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        using var valid = new MultipartFormDataContent();
        var image = new ByteArrayContent(png);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        valid.Add(image, "file", "my evidence.png");
        var uploadRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/me/assignments/{assignment.Id}/uploads")
        { Content = valid };
        Authorize(uploadRequest, ownerToken);
        var uploaded = await Client.SendAsync(uploadRequest);
        Assert.That(uploaded.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var file = await uploaded.Content.ReadFromJsonAsync<UploadDto>();

        var submit = await PostAsync(
            $"/api/me/assignments/{assignment.Id}/submissions",
            new { photoUploadId = file!.Id, isCodingProject = false, attest = true },
            ownerToken);
        Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var download = await GetAsync($"/api/me/files/{file.Id}", ownerToken);
        Assert.Multiple(() =>
        {
            Assert.That(download.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(download.Content.Headers.ContentType!.MediaType, Is.EqualTo("image/png"));
        });
        var forbidden = await GetAsync($"/api/me/files/{file.Id}", otherToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-CONSENT-001")]
    [Property("Req", "L2-CONSENT-004")]
    public async Task Individual_photo_storage_and_use_follow_granular_media_consent_live()
    {
        // Traces to: L2-CONSENT-001 AC2/AC4, L2-CONSENT-004 AC3
        var (youthId, youthToken, guardianToken) = await SeedConsentedMinorAsync();
        var assignment = await SeedAssignmentAsync("photo");

        var groupOnly = await UploadPhotoAsync(assignment.Id, youthToken);
        Assert.Multiple(() =>
        {
            Assert.That(groupOnly.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(groupOnly.Headers.GetValues("X-WordUp-State").Single(),
                Is.EqualTo("media-consent-required"));
        });

        var permitIndividual = await PutAsync(
            $"/api/guardian/children/{youthId}/media-consent",
            new
            {
                mediaConsent = true,
                mediaAllPhotosVideo = true,
                mediaGroupOnly = false,
                mediaFirstNameCaptions = false,
            },
            guardianToken);
        Assert.That(permitIndividual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var uploaded = await UploadPhotoAsync(assignment.Id, youthToken);
        Assert.That(uploaded.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var file = await uploaded.Content.ReadFromJsonAsync<UploadDto>();
        var submitted = await PostAsync(
            $"/api/me/assignments/{assignment.Id}/submissions",
            new { photoUploadId = file!.Id, isCodingProject = false, attest = true },
            youthToken);
        var submission = await submitted.Content.ReadFromJsonAsync<SubmissionResultDto>();
        var visible = await GetAsync($"/api/me/submissions/{submission!.Id}", youthToken);
        Assert.That((await visible.Content.ReadFromJsonAsync<SubmissionStatusDto>())!.PhotoUrl,
            Is.EqualTo(file.Url));
        var administratorToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var visibleToStaff = await GetAsync($"/api/mentor/submissions/{submission.Id}", administratorToken);
        Assert.That((await visibleToStaff.Content.ReadFromJsonAsync<SubmissionReviewDto>())!.PhotoUrl,
            Is.EqualTo(file.Url));

        var revokeMedia = await PutAsync(
            $"/api/guardian/children/{youthId}/media-consent",
            new
            {
                mediaConsent = false,
                mediaAllPhotosVideo = false,
                mediaGroupOnly = false,
                mediaFirstNameCaptions = false,
            },
            guardianToken);
        Assert.That(revokeMedia.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var hidden = await GetAsync($"/api/me/submissions/{submission.Id}", youthToken);
        Assert.That((await hidden.Content.ReadFromJsonAsync<SubmissionStatusDto>())!.PhotoUrl, Is.Null);
        var hiddenFromStaff = await GetAsync($"/api/mentor/submissions/{submission.Id}", administratorToken);
        Assert.That((await hiddenFromStaff.Content.ReadFromJsonAsync<SubmissionReviewDto>())!.PhotoUrl, Is.Null);
        var deniedFile = await GetAsync(file.Url, youthToken);
        Assert.Multiple(() =>
        {
            Assert.That(deniedFile.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(deniedFile.Headers.GetValues("X-WordUp-State").Single(),
                Is.EqualTo("media-consent-required"));
        });
    }

    [Test]
    [Property("Req", "L2-LEARN-006")]
    [Property("Req", "L2-NFR-001")]
    public async Task Pdf_evidence_is_sniffed_linked_to_the_submitted_copy_and_privately_served()
    {
        // Traces to: L2-LEARN-006 AC2/AC4, L2-NFR-001 AC4
        var (ownerId, ownerToken) = await SeedYouthAsync();
        var (_, otherYouthToken) = await SeedYouthAsync();
        var assignedMentor = await SeedAssignedMentorAsync(ownerId);
        var administratorToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var assignment = await SeedAssignmentAsync("file");

        var disguised = await UploadPdfAsync(
            assignment.Id,
            ownerToken,
            "evidence.pdf",
            "<script>alert(1)</script>"u8.ToArray());
        Assert.That(disguised.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var pdf = "%PDF-1.4\n1 0 obj\n<<>>\nendobj\ntrailer\n<<>>\n%%EOF"u8.ToArray();
        var unusedResponse = await UploadPdfAsync(assignment.Id, ownerToken, "draft.pdf", pdf);
        var selectedResponse = await UploadPdfAsync(assignment.Id, ownerToken, "evidence.pdf", pdf);
        Assert.Multiple(() =>
        {
            Assert.That(unusedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(selectedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
        var unused = await unusedResponse.Content.ReadFromJsonAsync<UploadDto>();
        var selected = await selectedResponse.Content.ReadFromJsonAsync<UploadDto>();
        Assert.That(selected!.Kind, Is.EqualTo("file"));

        var submitted = await PostAsync(
            $"/api/me/assignments/{assignment.Id}/submissions",
            new { fileUploadId = selected.Id, isCodingProject = false, attest = true },
            ownerToken);
        Assert.That(submitted.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await submitted.Content.ReadFromJsonAsync<SubmissionResultDto>();

        var status = await GetAsync($"/api/me/submissions/{result!.Id}", ownerToken);
        var submittedCopy = await status.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.Multiple(() =>
        {
            Assert.That(submittedCopy!.File, Is.Not.Null);
            Assert.That(submittedCopy.File!.FileName, Is.EqualTo("evidence.pdf"));
            Assert.That(submittedCopy.File.Url, Is.EqualTo(selected.Url));
        });
        var staffReview = await GetAsync($"/api/mentor/submissions/{result.Id}", assignedMentor.Token);
        var reviewCopy = await staffReview.Content.ReadFromJsonAsync<SubmissionReviewDto>();
        Assert.Multiple(() =>
        {
            Assert.That(staffReview.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(reviewCopy!.File!.FileName, Is.EqualTo("evidence.pdf"));
            Assert.That(reviewCopy.File.Url, Is.EqualTo(selected.Url));
        });

        var ownerDownload = await GetAsync(selected.Url, ownerToken);
        Assert.Multiple(() =>
        {
            Assert.That(ownerDownload.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(ownerDownload.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/pdf"));
            Assert.That(ownerDownload.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
            Assert.That(ownerDownload.Content.Headers.ContentDisposition!.FileNameStar, Is.EqualTo("evidence.pdf"));
        });
        Assert.That(await ownerDownload.Content.ReadAsByteArrayAsync(), Is.EqualTo(pdf));

        var unusedAfterSubmit = await GetAsync(unused!.Url, ownerToken);
        Assert.That(unusedAfterSubmit.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That((await GetAsync(selected.Url, otherYouthToken)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That((await GetAsync(selected.Url, assignedMentor.Token)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await GetAsync(selected.Url, administratorToken)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await Client.GetAsync(selected.Url)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    [Property("Req", "L2-LEARN-007")]
    public async Task Submission_status_is_owner_only()
    {
        var (_, ownerToken) = await SeedYouthAsync();
        var (_, otherToken) = await SeedYouthAsync();
        var assignment = await SeedAssignmentAsync();

        var created = await PostAsync(
            $"/api/me/assignments/{assignment.Id}/submissions",
            new { bodyText = "Mine", isCodingProject = false, attest = true },
            ownerToken);
        var dto = await created.Content.ReadFromJsonAsync<SubmissionResultDto>();

        // Owner can read it.
        var mine = await GetAsync($"/api/me/submissions/{dto!.Id}", ownerToken);
        Assert.That(mine.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Traces to: L2-LEARN-007 (a different youth cannot read another's submission)
        var theirs = await GetAsync($"/api/me/submissions/{dto.Id}", otherToken);
        Assert.That(theirs.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private sealed record SubmissionResultDto(Guid Id);

    private sealed record AssignmentBriefDto(
        Guid Id,
        string Title,
        string TaskDescription,
        DateTime? DueDate,
        List<string> AllowedSubmissionTypes,
        List<RubricDto> Rubric,
        int TotalPoints,
        int MaxUploadBytes,
        Guid? ExistingSubmissionId,
        string? ExistingSubmissionStatus);

    private sealed record RubricDto(Guid Id, string Description, int Points);

    private sealed record UploadDto(
        Guid Id,
        string FileName,
        string ContentType,
        long SizeBytes,
        string Url,
        string? Kind = null);

    private sealed record SubmissionReviewDto(
        string? PhotoUrl,
        SubmissionAttachmentDto? File = null);

    private sealed record TimelineDto(string Type, string Label, DateTime? At, bool Current);

    private sealed record SubmissionStatusDto(
        Guid Id,
        Guid AssignmentId,
        string AssignmentTitle,
        string Status,
        string? BodyText,
        string? PhotoUrl,
        string? LinkUrl,
        string? MentorComment,
        int? TotalAwarded,
        int TotalPossible,
        DateTime SubmittedAt,
        DateTime? ReviewedAt,
        List<TimelineDto> Timeline,
        SubmissionAttachmentDto? File = null);

    private sealed record SubmissionAttachmentDto(
        Guid Id,
        string FileName,
        string ContentType,
        long SizeBytes,
        string Url);
}

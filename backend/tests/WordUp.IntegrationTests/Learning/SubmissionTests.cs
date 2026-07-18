using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
    public async Task Submitting_without_attestation_is_rejected_and_with_attestation_succeeds()
    {
        var (_, youthToken) = await SeedYouthAsync();
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
        Assert.That(ok.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var created = await ok.Content.ReadFromJsonAsync<SubmissionResultDto>();
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
            $"/api/me/assignments/{assignment.Id}/uploads") { Content = fake };
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
            $"/api/me/assignments/{assignment.Id}/uploads") { Content = valid };
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
        string Url);

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
        List<TimelineDto> Timeline);
}

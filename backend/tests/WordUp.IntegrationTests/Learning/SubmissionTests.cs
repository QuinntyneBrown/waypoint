using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Domain;
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
        var auth = await signIn.Content.ReadFromJsonAsync<WordUp.Application.Auth.AuthResult>();
        return (youthId, auth!.AccessToken);
    }

    private async Task<Guid> SeedAssignmentAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var assignment = new Assignment
        {
            Title = "Memory Verse",
            TaskDescription = "Write out this week's verse.",
            SubmissionTypes = "text",
            TotalPoints = 10,
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        return assignment.Id;
    }

    [Test]
    [Property("Req", "L2-LEARN-006")]
    public async Task Submitting_without_attestation_is_rejected_and_with_attestation_succeeds()
    {
        var (_, youthToken) = await SeedYouthAsync();
        var assignmentId = await SeedAssignmentAsync();

        // Traces to: L2-LEARN-006 (attestation required)
        var noAttest = await PostAsync(
            $"/api/me/assignments/{assignmentId}/submissions",
            new { bodyText = "My answer", isCodingProject = false, attest = false },
            youthToken);
        Assert.That(noAttest.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var ok = await PostAsync(
            $"/api/me/assignments/{assignmentId}/submissions",
            new { bodyText = "My answer", isCodingProject = false, attest = true },
            youthToken);
        Assert.That(ok.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var created = await ok.Content.ReadFromJsonAsync<SubmissionResultDto>();
        var status = await GetAsync($"/api/me/submissions/{created!.Id}", youthToken);
        Assert.That(status.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var view = await status.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.That(view!.Status, Is.EqualTo(SubmissionStatus.Submitted.ToString()));
    }

    [Test]
    [Property("Req", "L2-LEARN-007")]
    public async Task Submission_status_is_owner_only()
    {
        var (_, ownerToken) = await SeedYouthAsync();
        var (_, otherToken) = await SeedYouthAsync();
        var assignmentId = await SeedAssignmentAsync();

        var created = await PostAsync(
            $"/api/me/assignments/{assignmentId}/submissions",
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

    private sealed record SubmissionStatusDto(
        Guid Id, string Status, string? MentorComment, int? TotalAwarded, DateTime SubmittedAt, DateTime? ReviewedAt);
}

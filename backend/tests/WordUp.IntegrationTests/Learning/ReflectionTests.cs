using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-003 (youth reflections)
[TestFixture]
[Property("Req", "L2-LEARN-003")]
public sealed class ReflectionTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private sealed record ReflectionResultDto(Guid Id);

    [Test]
    [Property("Req", "L2-LEARN-003")]
    public async Task Reflection_defaults_private_allows_empty_drafts_and_rejects_empty_submissions()
    {
        // Traces to: L2-LEARN-003
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        // Save a real reflection not shared with the mentor -> stored Private.
        var privateLesson = Guid.NewGuid();
        var saved = await PostAsync("/api/me/reflections", new
        {
            lessonId = privateLesson,
            text = "God is faithful.",
            mood = "grateful",
            shareWithMentor = false,
            isDraft = false,
        }, youthToken);
        Assert.That(saved.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var reflectionId = (await saved.Content.ReadFromJsonAsync<ReflectionResultDto>())!.Id;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = await db.Reflections.FirstAsync(r => r.Id == reflectionId);
            Assert.That(stored.Visibility, Is.EqualTo(Visibility.Private));
        }

        // A draft may have empty text.
        var draft = await PostAsync("/api/me/reflections", new
        {
            lessonId = Guid.NewGuid(),
            text = (string?)null,
            mood = (string?)null,
            shareWithMentor = false,
            isDraft = true,
        }, youthToken);
        Assert.That(draft.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // A non-draft submission with empty text is rejected.
        var invalid = await PostAsync("/api/me/reflections", new
        {
            lessonId = Guid.NewGuid(),
            text = "",
            mood = (string?)null,
            shareWithMentor = false,
            isDraft = false,
        }, youthToken);
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Property("Req", "L2-LEARN-003")]
    public async Task A_mentor_cannot_use_the_youth_reflection_route()
    {
        // Traces to: L2-LEARN-003
        var mentorToken = await CreateUserAndGetTokenAsync(Roles.Mentor);

        var response = await PostAsync("/api/me/reflections", new
        {
            lessonId = Guid.NewGuid(),
            text = "Not allowed.",
            mood = (string?)null,
            shareWithMentor = false,
            isDraft = false,
        }, mentorToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}

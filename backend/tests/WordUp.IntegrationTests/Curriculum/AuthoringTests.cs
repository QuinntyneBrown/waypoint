using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Curriculum;

// Acceptance Test
// Traces to: L2-CURRIC-001 (tracks/modules authoring, ordering, publish),
//            L2-CURRIC-002 (lessons authoring, staff view, soft-delete)
[TestFixture]
[Property("Req", "L2-CURRIC-001")]
public sealed class AuthoringTests : IntegrationTestBase
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

    private async Task<HttpResponseMessage> GetAsync(string url, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, accessToken);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> DeleteAsync(string url, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        Authorize(request, accessToken);
        return await Client.SendAsync(request);
    }

    // Traces to: L2-CURRIC-001 (AC1 unique key, authorization)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    public async Task Admin_creates_a_track_duplicate_key_conflicts_and_youth_is_forbidden()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var key = $"bible-{Guid.NewGuid():N}";

        var created = await PostAsync("/api/tracks", new { name = "Bible Foundations", key, description = "OT/NT" }, adminToken);
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await created.Content.ReadFromJsonAsync<TrackResultDto>();
        Assert.That(result!.Id, Is.Not.EqualTo(Guid.Empty));

        // AC1: a second track with the same key is rejected.
        var duplicate = await PostAsync("/api/tracks", new { name = "Bible Again", key }, adminToken);
        Assert.That(duplicate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        // Authorization: a youth cannot author curriculum.
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await PostAsync("/api/tracks", new { name = "Nope", key = $"x-{Guid.NewGuid():N}" }, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    // Traces to: L2-CURRIC-001 (AC2 create modules, AC3 reorder persists)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    public async Task Modules_can_be_created_and_reordered_and_the_new_order_persists()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks", new { name = "Leadership", key = $"lead-{Guid.NewGuid():N}" }, adminToken))
            .Content.ReadFromJsonAsync<TrackResultDto>())!.Id;

        var first = (await (await PostAsync("/api/modules", new { trackId, name = "Module A" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var second = (await (await PostAsync("/api/modules", new { trackId, name = "Module B" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;

        // Reverse the order.
        var reorder = await PostAsync(
            "/api/modules/reorder", new { trackId, orderedModuleIds = new[] { second, first } }, adminToken);
        Assert.That(reorder.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // AC3: the new order is persisted.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var firstModule = await db.Modules.FirstAsync(m => m.Id == first);
        var secondModule = await db.Modules.FirstAsync(m => m.Id == second);
        Assert.That(secondModule.Order, Is.EqualTo(1));
        Assert.That(firstModule.Order, Is.EqualTo(2));
    }

    // Traces to: L2-CURRIC-001 (AC4 publish a module)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    public async Task Publishing_a_module_sets_its_status_to_published()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks", new { name = "Coding", key = $"code-{Guid.NewGuid():N}" }, adminToken))
            .Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync("/api/modules", new { trackId, name = "Intro" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;

        var publish = await PostAsync(
            $"/api/modules/{moduleId}/publish", new { status = (int)PublishStatus.Published }, adminToken);
        Assert.That(publish.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = await db.Modules.FirstAsync(m => m.Id == moduleId);
        Assert.That(module.Status, Is.EqualTo(PublishStatus.Published));
    }

    // Traces to: L2-CURRIC-002 (AC1 lesson persists learning objectives + leader guide + teaching body)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Creating_a_lesson_persists_all_authoring_fields_and_the_staff_view_returns_them()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks", new { name = "Discipleship", key = $"disc-{Guid.NewGuid():N}" }, adminToken))
            .Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync("/api/modules", new { trackId, name = "Week 1" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;

        var lessonId = (await (await PostAsync("/api/lessons", new
        {
            moduleId,
            title = "The Sermon on the Mount",
            scriptureReference = "Matthew 5-7",
            estimatedMinutes = 45,
            learningObjectives = "Understand the Beatitudes.",
            teachingBody = "The core teaching narrative.",
            reflectionPrompts = "What does mercy mean to you?",
            leaderGuideNotes = "Encourage quiet reflection first.",
        }, adminToken)).Content.ReadFromJsonAsync<LessonResultDto>())!.Id;

        var get = await GetAsync($"/api/lessons/{lessonId}", adminToken);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var detail = await get.Content.ReadFromJsonAsync<LessonDetailDto>();
        Assert.That(detail!.LearningObjectives, Is.EqualTo("Understand the Beatitudes."));
        Assert.That(detail.TeachingBody, Is.EqualTo("The core teaching narrative."));
        Assert.That(detail.LeaderGuideNotes, Is.EqualTo("Encourage quiet reflection first."));
        Assert.That(detail.ReflectionPrompts, Is.EqualTo("What does mercy mean to you?"));
        Assert.That(detail.EstimatedMinutes, Is.EqualTo(45));
    }

    // Traces to: L2-CURRIC-002 (AC5 soft-delete: staff view 404 but row retained)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Deleting_a_lesson_soft_archives_it_so_the_view_is_gone_but_the_row_remains()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks", new { name = "Service", key = $"serv-{Guid.NewGuid():N}" }, adminToken))
            .Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync("/api/modules", new { trackId, name = "Outreach" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var lessonId = (await (await PostAsync(
            "/api/lessons", new { moduleId, title = "Serving Others" }, adminToken))
            .Content.ReadFromJsonAsync<LessonResultDto>())!.Id;

        var delete = await DeleteAsync($"/api/lessons/{lessonId}", adminToken);
        Assert.That(delete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // AC5: the staff view treats an archived lesson as gone.
        var get = await GetAsync($"/api/lessons/{lessonId}", adminToken);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        // AC5: the row is retained with ArchivedAt set (progress/linked quizzes are preserved).
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var lesson = await db.Lessons.FirstAsync(l => l.Id == lessonId);
        Assert.That(lesson.ArchivedAt, Is.Not.Null);
    }

    private sealed record TrackResultDto(Guid Id);
    private sealed record ModuleResultDto(Guid Id);
    private sealed record LessonResultDto(Guid Id);
    private sealed record LessonDetailDto(
        Guid Id,
        Guid ModuleId,
        string Title,
        string? ScriptureReference,
        int? EstimatedMinutes,
        string? LearningObjectives,
        string? TeachingBody,
        string? ReflectionPrompts,
        string? LeaderGuideNotes,
        string Status);
}

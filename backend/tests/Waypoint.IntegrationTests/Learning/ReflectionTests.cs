using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-003 (youth reflections)
[TestFixture]
[Property("Req", "L2-LEARN-003")]
public sealed class ReflectionTests : IntegrationTestBase
{
    private sealed record Seed(
        Guid LessonId,
        Guid YouthId,
        string YouthToken,
        string MentorToken,
        string OtherMentorToken);

    private async Task<Seed> SeedLessonAndYouthAsync()
    {
        var track = new Track
        {
            Name = "Reflection track",
            Key = $"reflection-{Guid.NewGuid():N}",
            Status = PublishStatus.Published,
        };
        var module = new Module
        {
            TrackId = track.Id,
            Name = "Listening well",
            Order = 1,
            Status = PublishStatus.Published,
        };
        var lesson = new Lesson
        {
            ModuleId = module.Id,
            Title = "A neighbour who listens",
            ReflectionPrompts = "Where did you notice someone who needed to be heard?",
            Order = 1,
            Status = PublishStatus.Published,
        };
        var cohort = new Cohort { Name = "Reflection cohort", TrackId = track.Id };
        var mentorEmail = $"mentor.{Guid.NewGuid():N}@example.com";
        var otherMentorEmail = $"mentor.{Guid.NewGuid():N}@example.com";
        var youthEmail = $"youth.{Guid.NewGuid():N}@example.com";
        var mentor = new User
        {
            Email = mentorEmail,
            DisplayName = "Assigned Mentor",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        var otherMentor = new User
        {
            Email = otherMentorEmail,
            DisplayName = "Other Mentor",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        var youth = new User
        {
            Email = youthEmail,
            DisplayName = "Reflective Youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CohortId = cohort.Id,
            MentorId = mentor.Id,
        };

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var passwordHash = hasher.Hash("Passw0rd!");
            mentor.PasswordHash = passwordHash;
            otherMentor.PasswordHash = passwordHash;
            youth.PasswordHash = passwordHash;
            db.Tracks.Add(track);
            db.Modules.Add(module);
            db.Lessons.Add(lesson);
            db.Cohorts.Add(cohort);
            db.Users.AddRange(mentor, otherMentor, youth);
            await db.SaveChangesAsync();
        }

        async Task<string> SignInAsync(string email)
        {
            var response = await Client.PostAsJsonAsync(
                "/api/auth/sign-in",
                new { email, password = "Passw0rd!" });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<AuthResult>())!.AccessToken;
        }

        return new Seed(
            lesson.Id,
            youth.Id,
            await SignInAsync(youthEmail),
            await SignInAsync(mentorEmail),
            await SignInAsync(otherMentorEmail));
    }

    private async Task<HttpResponseMessage> PostAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private sealed record ReflectionResultDto(Guid Id);

    private sealed record ReflectionEditorDto(
        Guid LessonId,
        string LessonTitle,
        string? Prompt,
        ReflectionEntryDto? Reflection);

    private sealed record ReflectionEntryDto(
        Guid Id,
        string? Text,
        string? Mood,
        IReadOnlyDictionary<string, string> SecondaryResponses,
        bool ShareWithMentor,
        bool IsDraft,
        DateTime UpdatedAt);

    private sealed record SharedReflectionDto(Guid Id, Guid YouthId, string Text, string Mood);

    [Test]
    [Property("Req", "L2-LEARN-003")]
    public async Task Saved_draft_is_restored_and_only_an_assigned_mentor_can_read_a_shared_submission()
    {
        var seed = await SeedLessonAndYouthAsync();

        var initialRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/me/reflections/lesson/{seed.LessonId}");
        Authorize(initialRequest, seed.YouthToken);
        var initial = await Client.SendAsync(initialRequest);
        Assert.That(initial.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var emptyEditor = await initial.Content.ReadFromJsonAsync<ReflectionEditorDto>();
        Assert.Multiple(() =>
        {
            Assert.That(emptyEditor!.LessonTitle, Is.EqualTo("A neighbour who listens"));
            Assert.That(emptyEditor.Prompt, Does.Contain("needed to be heard"));
            Assert.That(emptyEditor.Reflection, Is.Null);
        });

        var draft = await PostAsync("/api/me/reflections", new
        {
            lessonId = seed.LessonId,
            text = "I noticed my friend was quiet.",
            mood = "thoughtful",
            secondaryResponses = new Dictionary<string, string>
            {
                ["One action I will take"] = "Ask before offering advice.",
            },
            shareWithMentor = false,
            isDraft = true,
        }, seed.YouthToken);
        Assert.That(draft.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var restoreRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/me/reflections/lesson/{seed.LessonId}");
        Authorize(restoreRequest, seed.YouthToken);
        var restored = await Client.SendAsync(restoreRequest);
        var editor = await restored.Content.ReadFromJsonAsync<ReflectionEditorDto>();
        Assert.Multiple(() =>
        {
            Assert.That(editor!.Reflection!.Text, Is.EqualTo("I noticed my friend was quiet."));
            Assert.That(editor.Reflection.Mood, Is.EqualTo("thoughtful"));
            Assert.That(editor.Reflection.SecondaryResponses["One action I will take"],
                Is.EqualTo("Ask before offering advice."));
            Assert.That(editor.Reflection.ShareWithMentor, Is.False);
            Assert.That(editor.Reflection.IsDraft, Is.True);
        });

        var privateMentorRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/mentor/youth/{seed.YouthId}/reflections");
        Authorize(privateMentorRequest, seed.MentorToken);
        var privateMentorView = await Client.SendAsync(privateMentorRequest);
        Assert.That(await privateMentorView.Content.ReadFromJsonAsync<List<SharedReflectionDto>>(), Is.Empty);

        var submit = await PostAsync("/api/me/reflections", new
        {
            lessonId = seed.LessonId,
            text = "Listening first helped my friend open up.",
            mood = "hopeful",
            secondaryResponses = new Dictionary<string, string>(),
            shareWithMentor = true,
            isDraft = false,
        }, seed.YouthToken);
        Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var mentorRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/mentor/youth/{seed.YouthId}/reflections");
        Authorize(mentorRequest, seed.MentorToken);
        var mentorView = await Client.SendAsync(mentorRequest);
        Assert.That(mentorView.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var shared = await mentorView.Content.ReadFromJsonAsync<List<SharedReflectionDto>>();
        Assert.Multiple(() =>
        {
            Assert.That(shared, Has.Count.EqualTo(1));
            Assert.That(shared![0].Text, Does.Contain("helped my friend"));
            Assert.That(shared[0].Mood, Is.EqualTo("hopeful"));
        });

        var otherMentorRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/mentor/youth/{seed.YouthId}/reflections");
        Authorize(otherMentorRequest, seed.OtherMentorToken);
        var forbidden = await Client.SendAsync(otherMentorRequest);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-LEARN-003")]
    public async Task Reflection_defaults_private_allows_empty_drafts_and_rejects_empty_submissions()
    {
        // Traces to: L2-LEARN-003
        var seed = await SeedLessonAndYouthAsync();
        var youthToken = seed.YouthToken;

        // Save a real reflection not shared with the mentor -> stored Private.
        var privateLesson = seed.LessonId;
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
            lessonId = seed.LessonId,
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

        var arbitraryLesson = await PostAsync("/api/me/reflections", new
        {
            lessonId = Guid.NewGuid(),
            text = "This lesson is not mine.",
            mood = "unsure",
            shareWithMentor = false,
            isDraft = false,
        }, youthToken);
        Assert.That(arbitraryLesson.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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

using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Domain.CurriculumAuthoring;
using Waypoint.IntegrationTests.Curriculum;

namespace Waypoint.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-002 (AC5 adaptive playback address, AC6 signed time-limited access)
[TestFixture]
[Property("Req", "L2-LEARN-002")]
public sealed partial class VideoPlaybackTests : VideoTestBase
{
    private sealed record Seeded(
        string YouthToken, string MentorToken, Guid LessonId, Guid OtherLessonId,
        Guid ReadyId, Guid QueuedId, Guid DocumentId);

    private sealed record Fixture(
        string YouthEmail, string YouthPassword, string MentorEmail, string MentorPassword,
        Guid FirstLessonId, Guid SecondLessonId);

    private sealed record PlaybackDto(string Url, DateTime ExpiresAt, double DurationSeconds, string PosterUrl);

    private sealed record LessonMediaDto(Guid MediaId, string MediaType, string VideoStatus, double? DurationSeconds);

    private sealed record LessonDto(IReadOnlyList<LessonMediaDto> Media);

    private async Task<string> SignInAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthDto>())!.AccessToken;
    }

    private sealed record AuthDto(string AccessToken);

    private async Task<Seeded> SeedAsync()
    {
        var response = await Client.PostAsync("/api/_test/youth-curriculum", null);
        response.EnsureSuccessStatusCode();
        var fixture = (await response.Content.ReadFromJsonAsync<Fixture>())!;
        var ready = new LessonMedia
        {
            LessonId = fixture.SecondLessonId, MediaType = "video", VideoStatus = VideoStatus.Ready,
            DurationSeconds = 4332, Width = 1280, Height = 720, OriginalFileName = "week.mp4",
            ContentType = "video/mp4",
        };
        var queued = new LessonMedia
        {
            LessonId = fixture.SecondLessonId, MediaType = "video", VideoStatus = VideoStatus.Queued,
            OriginalFileName = "next.mp4", ContentType = "video/mp4",
        };
        var document = new LessonMedia
        {
            LessonId = fixture.SecondLessonId, MediaType = "document", Url = "/api/lesson-media/x",
            OriginalFileName = "handout.pdf", ContentType = "application/pdf",
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            db.LessonMedia.AddRange(ready, queued, document);
            await db.SaveChangesAsync();
        }
        return new Seeded(
            await SignInAsync(fixture.YouthEmail, fixture.YouthPassword),
            await SignInAsync(fixture.MentorEmail, fixture.MentorPassword),
            fixture.SecondLessonId, fixture.FirstLessonId, ready.Id, queued.Id, document.Id);
    }

    [GeneratedRegex(@"^/api/media/[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+/(?<id>[0-9a-f-]{36})/(?<file>.+)$")]
    private static partial Regex SignedUrl();

    // Traces to: L2-LEARN-002 (AC5, AC6)
    [Test]
    [Property("Req", "L2-LEARN-002")]
    public async Task An_enrolled_youth_receives_a_signed_expiring_playback_address_for_a_ready_video()
    {
        var seeded = await SeedAsync();

        var response = await SendAsync(
            HttpMethod.Get, $"/api/me/lessons/{seeded.LessonId}/media/{seeded.ReadyId}/playback", seeded.YouthToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var playback = (await response.Content.ReadFromJsonAsync<PlaybackDto>())!;
        var master = SignedUrl().Match(playback.Url);
        var poster = SignedUrl().Match(playback.PosterUrl);
        Assert.Multiple(() =>
        {
            Assert.That(master.Success, Is.True);
            Assert.That(master.Groups["id"].Value, Is.EqualTo(seeded.ReadyId.ToString()));
            Assert.That(master.Groups["file"].Value, Is.EqualTo("master.m3u8"));
            Assert.That(poster.Groups["file"].Value, Is.EqualTo("poster.jpg"));
            Assert.That(playback.DurationSeconds, Is.EqualTo(4332));
            Assert.That(
                playback.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddHours(8)).Within(TimeSpan.FromMinutes(2)));
        });
    }

    // Traces to: L2-LEARN-002 (AC1 lesson shows optional media, AC5 video status drives the player)
    [Test]
    [Property("Req", "L2-LEARN-002")]
    public async Task The_lesson_lists_each_media_item_with_its_video_status_and_duration()
    {
        var seeded = await SeedAsync();

        var lesson = await (await SendAsync(HttpMethod.Get, $"/api/me/lessons/{seeded.LessonId}", seeded.YouthToken))
            .Content.ReadFromJsonAsync<LessonDto>();

        Assert.Multiple(() =>
        {
            var ready = lesson!.Media.Single(item => item.MediaId == seeded.ReadyId);
            Assert.That(ready.VideoStatus, Is.EqualTo("Ready"));
            Assert.That(ready.DurationSeconds, Is.EqualTo(4332));
            Assert.That(lesson.Media.Single(item => item.MediaId == seeded.QueuedId).VideoStatus, Is.EqualTo("Queued"));
            Assert.That(lesson.Media.Single(item => item.MediaId == seeded.DocumentId).VideoStatus, Is.EqualTo("None"));
        });
    }

    // Traces to: L2-LEARN-002 (AC6 only valid access is issued a signed address)
    [Test]
    [Property("Req", "L2-LEARN-002")]
    public async Task A_playback_address_is_refused_unless_the_video_is_ready_and_the_lesson_is_accessible()
    {
        var seeded = await SeedAsync();
        string Url(Guid lesson, Guid media) => $"/api/me/lessons/{lesson}/media/{media}/playback";

        var notReady = await SendAsync(HttpMethod.Get, Url(seeded.LessonId, seeded.QueuedId), seeded.YouthToken);
        var document = await SendAsync(HttpMethod.Get, Url(seeded.LessonId, seeded.DocumentId), seeded.YouthToken);
        var unknown = await SendAsync(HttpMethod.Get, Url(seeded.LessonId, Guid.NewGuid()), seeded.YouthToken);
        var wrongLesson = await SendAsync(HttpMethod.Get, Url(seeded.OtherLessonId, seeded.ReadyId), seeded.YouthToken);
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var draftLessonId = await CreateLessonAsync(adminToken);
        var draftLesson = await SendAsync(HttpMethod.Get, Url(draftLessonId, seeded.ReadyId), seeded.YouthToken);
        var anonymous = await SendAsync(HttpMethod.Get, Url(seeded.LessonId, seeded.ReadyId), null);
        var guardianToken = await CreateUserAndGetTokenAsync(Roles.Guardian);
        var guardian = await SendAsync(HttpMethod.Get, Url(seeded.LessonId, seeded.ReadyId), guardianToken);

        Assert.Multiple(() =>
        {
            Assert.That(notReady.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(document.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(unknown.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(wrongLesson.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(draftLesson.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(guardian.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        });
    }

    // Traces to: L2-LEARN-002 (AC6), L2-CURRIC-002 (AC5 staff roles)
    [Test]
    [Property("Req", "L2-LEARN-002")]
    public async Task Administrators_and_mentors_may_preview_a_ready_video_but_other_roles_may_not()
    {
        var seeded = await SeedAsync();
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var guardianToken = await CreateUserAndGetTokenAsync(Roles.Guardian);
        var url = $"/api/lesson-media/{seeded.ReadyId}/playback";

        var admin = await SendAsync(HttpMethod.Get, url, adminToken);
        var mentor = await SendAsync(HttpMethod.Get, url, seeded.MentorToken);
        var guardian = await SendAsync(HttpMethod.Get, url, guardianToken);
        var notReady = await SendAsync(
            HttpMethod.Get, $"/api/lesson-media/{seeded.QueuedId}/playback", adminToken);

        Assert.Multiple(() =>
        {
            Assert.That(admin.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(mentor.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(guardian.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(notReady.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        });
    }
}

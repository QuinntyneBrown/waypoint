using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Domain.CurriculumAuthoring;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Curriculum;

// Acceptance Test
// Traces to: L2-CURRIC-002 (AC7 background transcode, status, retry)
[TestFixture]
[Property("Req", "L2-CURRIC-002")]
public sealed class VideoTranscodeTests : VideoTestBase
{
    private string MediaDir(string folder, Guid mediaId) =>
        Path.Combine(Factory.MediaRoot, folder, mediaId.ToString("N"));

    private async Task<(Guid LessonId, Guid MediaId)> QueueVideoAsync(string adminToken, string marker = "")
    {
        var (_, lessonId, session) = await UploadAllChunksAsync(
            adminToken, FakeMp4((int)(2 * MiB), marker));
        var completed = await CompleteAsync(adminToken, lessonId, session.UploadId);
        Assert.That(completed.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var media = await completed.Content.ReadFromJsonAsync<CompletedMediaDto>();
        return (lessonId, media!.Id);
    }

    private async Task RunProcessorAsync()
    {
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<VideoTranscodeProcessor>().RunOnceAsync();
    }

    private async Task SweepAsync()
    {
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<VideoTranscodeProcessor>().SweepAsync();
    }

    private async Task<(LessonMedia Media, VideoTranscodeJob? Job)> LoadAsync(Guid mediaId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        return (
            await db.LessonMedia.AsNoTracking().SingleAsync(item => item.Id == mediaId),
            await db.VideoTranscodeJobs.AsNoTracking().SingleOrDefaultAsync(job => job.MediaId == mediaId));
    }

    // Traces to: L2-CURRIC-002 (AC7 Queued becomes Ready with the ladder published)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task A_queued_video_is_transcoded_published_and_marked_ready()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, mediaId) = await QueueVideoAsync(adminToken);

        await RunProcessorAsync();

        var (media, job) = await LoadAsync(mediaId);
        Assert.Multiple(() =>
        {
            Assert.That(media.VideoStatus, Is.EqualTo(VideoStatus.Ready));
            Assert.That(media.ProgressPercent, Is.EqualTo(100));
            Assert.That(media.FailureCode, Is.Null);
            Assert.That(job, Is.Null);
            Assert.That(File.Exists(Path.Combine(MediaDir("hls", mediaId), "master.m3u8")), Is.True);
            Assert.That(Directory.Exists(MediaDir("work", mediaId)), Is.False);
        });
    }

    // Traces to: L2-CURRIC-002 (AC7 failed transcode is visible after two attempts)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task A_failing_transcode_is_retried_once_then_marked_failed()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, mediaId) = await QueueVideoAsync(adminToken, "FAILTRANSCODE");

        await RunProcessorAsync();
        var afterFirst = await LoadAsync(mediaId);
        await RunProcessorAsync();
        var afterSecond = await LoadAsync(mediaId);
        await RunProcessorAsync();
        var afterThird = await LoadAsync(mediaId);

        Assert.Multiple(() =>
        {
            Assert.That(afterFirst.Media.VideoStatus, Is.EqualTo(VideoStatus.Queued));
            Assert.That(afterFirst.Job!.AttemptCount, Is.EqualTo(1));
            Assert.That(afterSecond.Media.VideoStatus, Is.EqualTo(VideoStatus.Failed));
            Assert.That(afterSecond.Media.FailureCode, Is.EqualTo("TranscodeFailed"));
            Assert.That(afterSecond.Job!.AttemptCount, Is.EqualTo(2));
            Assert.That(afterThird.Job!.AttemptCount, Is.EqualTo(2));
            Assert.That(Directory.Exists(MediaDir("work", mediaId)), Is.False);
        });
    }

    // Traces to: L2-CURRIC-002 (AC7 a failed transcode can be retried), AC5 permission matrix
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task A_failed_video_can_be_retried_by_an_administrator_only()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var (lessonId, failedId) = await QueueVideoAsync(adminToken, "FAILTRANSCODE");
        await RunProcessorAsync();
        await RunProcessorAsync();
        var url = $"/api/lessons/{lessonId}/media/{failedId}/retry-transcode";
        var (readyLessonId, readyId) = await QueueVideoAsync(adminToken);
        await RunProcessorAsync();

        var youth = await SendAsync(HttpMethod.Post, url, youthToken);
        var notFailed = await SendAsync(
            HttpMethod.Post, $"/api/lessons/{readyLessonId}/media/{readyId}/retry-transcode", adminToken);
        var retried = await SendAsync(HttpMethod.Post, url, adminToken);
        var (media, job) = await LoadAsync(failedId);

        Assert.Multiple(() =>
        {
            Assert.That(youth.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(notFailed.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(retried.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(media.VideoStatus, Is.EqualTo(VideoStatus.Queued));
            Assert.That(media.FailureCode, Is.Null);
            Assert.That(job!.AttemptCount, Is.EqualTo(0));
        });
    }

    // Traces to: L2-CURRIC-002 (AC7 a crashed worker's job is reclaimed once its lease lapses)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task A_job_with_a_lapsed_lease_is_reclaimed_but_a_live_lease_is_left_alone()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, liveId) = await QueueVideoAsync(adminToken);
        var (_, lapsedId) = await QueueVideoAsync(adminToken);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            foreach (var (mediaId, lease) in new[]
            {
                (liveId, DateTime.UtcNow.AddMinutes(5)), (lapsedId, DateTime.UtcNow.AddMinutes(-5)),
            })
            {
                (await db.LessonMedia.SingleAsync(item => item.Id == mediaId)).VideoStatus =
                    VideoStatus.Processing;
                var job = await db.VideoTranscodeJobs.SingleAsync(item => item.MediaId == mediaId);
                job.LeaseId = Guid.NewGuid();
                job.LeaseUntil = lease;
            }
            await db.SaveChangesAsync();
        }

        await RunProcessorAsync();

        Assert.Multiple(() =>
        {
            Assert.That(LoadAsync(liveId).Result.Media.VideoStatus, Is.EqualTo(VideoStatus.Processing));
            Assert.That(LoadAsync(lapsedId).Result.Media.VideoStatus, Is.EqualTo(VideoStatus.Ready));
        });
    }

    // Traces to: L2-CURRIC-002 (AC2 media can be removed; AC7 files do not linger)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Deleting_a_video_moves_its_files_to_trash_and_the_sweep_clears_them()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (lessonId, mediaId) = await QueueVideoAsync(adminToken);
        await RunProcessorAsync();
        var orphan = Guid.NewGuid();
        Directory.CreateDirectory(MediaDir("hls", orphan));

        var removed = await SendAsync(
            HttpMethod.Delete, $"/api/lessons/{lessonId}/media/{mediaId}", adminToken);
        var movedToTrash = Directory.Exists(MediaDir("trash", mediaId));
        var stillPublished = Directory.Exists(MediaDir("hls", mediaId));
        await SweepAsync();

        Assert.Multiple(() =>
        {
            Assert.That(removed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(movedToTrash, Is.True);
            Assert.That(stillPublished, Is.False);
            Assert.That(Directory.Exists(MediaDir("trash", mediaId)), Is.False);
            Assert.That(Directory.Exists(MediaDir("hls", orphan)), Is.False);
            Assert.That(File.Exists(Path.Combine(Factory.MediaRoot, "sources", $"{mediaId:N}.mp4")), Is.False);
        });
    }
}

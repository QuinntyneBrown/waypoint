using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Domain.CurriculumAuthoring;

namespace Waypoint.IntegrationTests.Curriculum;

// Acceptance Test
// Traces to: L2-CURRIC-002 (AC6 resumable chunked video upload)
[TestFixture]
[Property("Req", "L2-CURRIC-002")]
public sealed class VideoUploadTests : VideoTestBase
{
    // Traces to: L2-CURRIC-002 (AC6)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Admin_starts_an_upload_session_and_receives_the_chunk_plan()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var lessonId = await CreateLessonAsync(adminToken);

        var response = await SendAsync(HttpMethod.Post, $"/api/lessons/{lessonId}/video-uploads",
            adminToken, new { fileName = "week-1.mp4", sizeBytes = 20 * MiB });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var session = await response.Content.ReadFromJsonAsync<UploadSessionDto>();
        Assert.Multiple(() =>
        {
            Assert.That(session!.UploadId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(session.ChunkSizeBytes, Is.EqualTo(8 * MiB));
            Assert.That(session.ChunkCount, Is.EqualTo(3));
        });
    }

    // Traces to: L2-CURRIC-002 (AC6, size limit), L2-NFR-001 (AC4 upload validation)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    [Property("Req", "L2-NFR-001")]
    public async Task Starting_an_upload_rejects_invalid_requests()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var lessonId = await CreateLessonAsync(adminToken);
        var url = $"/api/lessons/{lessonId}/video-uploads";

        var tooBig = await SendAsync(HttpMethod.Post, url, adminToken,
            new { fileName = "huge.mp4", sizeBytes = 21L * 1024 * MiB });
        var empty = await SendAsync(HttpMethod.Post, url, adminToken,
            new { fileName = "empty.mp4", sizeBytes = 0L });
        var wrongType = await SendAsync(HttpMethod.Post, url, adminToken,
            new { fileName = "script.exe", sizeBytes = MiB });
        var unknownLesson = await SendAsync(HttpMethod.Post,
            $"/api/lessons/{Guid.NewGuid()}/video-uploads", adminToken,
            new { fileName = "a.mp4", sizeBytes = MiB });

        Assert.Multiple(() =>
        {
            Assert.That(tooBig.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(empty.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(wrongType.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(unknownLesson.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    // Traces to: L2-CURRIC-002 (AC5 permission matrix: only Program Administrator edits curriculum)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Only_a_program_administrator_may_start_an_upload()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var lessonId = await CreateLessonAsync(adminToken);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var url = $"/api/lessons/{lessonId}/video-uploads";
        var body = new { fileName = "a.mp4", sizeBytes = MiB };

        var youth = await SendAsync(HttpMethod.Post, url, youthToken, body);
        var anonymous = await SendAsync(HttpMethod.Post, url, null, body);

        Assert.Multiple(() =>
        {
            Assert.That(youth.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    // Traces to: L2-CURRIC-002 (AC6 an interrupted upload resumes from the last accepted chunk)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Chunks_are_accepted_in_any_order_and_the_status_lists_the_received_indexes()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var lessonId = await CreateLessonAsync(adminToken);
        var session = await StartAsync(adminToken, lessonId, 20 * MiB);

        var last = await PutChunkAsync(adminToken, lessonId, session.UploadId, 2, (int)(4 * MiB));
        var first = await PutChunkAsync(adminToken, lessonId, session.UploadId, 0, (int)(8 * MiB));
        var repeated = await PutChunkAsync(adminToken, lessonId, session.UploadId, 0, (int)(8 * MiB));
        var status = await StatusAsync(adminToken, lessonId, session.UploadId);

        Assert.Multiple(() =>
        {
            Assert.That(last.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(repeated.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(status.ChunkCount, Is.EqualTo(3));
            Assert.That(status.ReceivedChunks, Is.EqualTo(new[] { 0, 2 }));
        });
    }

    // Traces to: L2-CURRIC-002 (AC6), L2-NFR-001 (AC4 validate size and range)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    [Property("Req", "L2-NFR-001")]
    public async Task A_chunk_with_a_bad_index_or_wrong_size_is_rejected_and_not_recorded()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var lessonId = await CreateLessonAsync(adminToken);
        var session = await StartAsync(adminToken, lessonId, 20 * MiB);

        var beyondEnd = await PutChunkAsync(adminToken, lessonId, session.UploadId, 3, 1024);
        var wrongSize = await PutChunkAsync(adminToken, lessonId, session.UploadId, 0, 1024);
        var shortLast = await PutChunkAsync(adminToken, lessonId, session.UploadId, 2, (int)(8 * MiB));
        var unknownUpload = await PutChunkAsync(adminToken, lessonId, Guid.NewGuid(), 0, 1024);
        var otherLesson = await PutChunkAsync(
            adminToken, await CreateLessonAsync(adminToken), session.UploadId, 0, (int)(8 * MiB));
        var status = await StatusAsync(adminToken, lessonId, session.UploadId);

        Assert.Multiple(() =>
        {
            Assert.That(beyondEnd.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(wrongSize.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(shortLast.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(unknownUpload.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(otherLesson.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(status.ReceivedChunks, Is.Empty);
        });
    }

    // Traces to: L2-CURRIC-002 (AC5 permission matrix)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Only_a_program_administrator_may_send_chunks_or_read_status()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var lessonId = await CreateLessonAsync(adminToken);
        var session = await StartAsync(adminToken, lessonId, 20 * MiB);
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var youthPut = await PutChunkAsync(youthToken, lessonId, session.UploadId, 0, (int)(8 * MiB));
        var anonymousPut = await PutChunkAsync(null, lessonId, session.UploadId, 0, (int)(8 * MiB));
        var youthStatus = await SendAsync(
            HttpMethod.Get, $"/api/lessons/{lessonId}/video-uploads/{session.UploadId}", youthToken);

        Assert.Multiple(() =>
        {
            Assert.That(youthPut.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(anonymousPut.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(youthStatus.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        });
    }

    // A tiny stand-in for an MP4: the "ftyp" box marker sits at offset 4, as in a real file.
    // Traces to: L2-CURRIC-002 (AC7 accepted upload is queued for background transcoding),
    //            L2-CONSENT-003 (AC1 audit of mutating admin action)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    [Property("Req", "L2-CONSENT-003")]
    public async Task Completing_an_upload_queues_the_video_and_audits_it()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, lessonId, session) = await UploadAllChunksAsync(adminToken, FakeMp4((int)(10 * MiB)));

        var response = await CompleteAsync(adminToken, lessonId, session.UploadId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var media = await response.Content.ReadFromJsonAsync<CompletedMediaDto>();
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var row = await db.LessonMedia.AsNoTracking().SingleAsync(item => item.Id == media!.Id);
        Assert.Multiple(() =>
        {
            Assert.That(media!.MediaType, Is.EqualTo("video"));
            Assert.That(row.LessonId, Is.EqualTo(lessonId));
            Assert.That(row.VideoStatus, Is.EqualTo(VideoStatus.Queued));
            Assert.That(row.SizeBytes, Is.EqualTo(10 * MiB));
            Assert.That(row.DurationSeconds, Is.GreaterThan(0));
            Assert.That(db.VideoTranscodeJobs.Count(job => job.MediaId == row.Id), Is.EqualTo(1));
            Assert.That(
                db.AuditEntries.Any(entry => entry.Action == "LessonVideoUploaded"
                    && entry.TargetId == row.Id),
                Is.True);
            Assert.That(
                File.Exists(Path.Combine(Factory.MediaRoot, "sources", $"{row.Id:N}.mp4")), Is.True);
        });
    }

    // Traces to: L2-CURRIC-002 (AC6 validated before accepting), L2-NFR-001 (AC4 type and content)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    [Property("Req", "L2-NFR-001")]
    public async Task Completing_an_invalid_upload_is_rejected_and_persists_nothing()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var missing = await UploadAllChunksAsync(adminToken, FakeMp4((int)(10 * MiB)), skipIndex: 1);
        var notMp4 = await UploadAllChunksAsync(adminToken, new byte[(int)(2 * MiB)]);
        var notVideo = await UploadAllChunksAsync(adminToken, FakeMp4((int)(2 * MiB), "NOTVIDEO"));

        var missingResponse = await CompleteAsync(adminToken, missing.LessonId, missing.Session.UploadId);
        var notMp4Response = await CompleteAsync(adminToken, notMp4.LessonId, notMp4.Session.UploadId);
        var notVideoResponse = await CompleteAsync(adminToken, notVideo.LessonId, notVideo.Session.UploadId);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(missingResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(notMp4Response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(notVideoResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(
                db.LessonMedia.Count(item => item.LessonId == missing.LessonId
                    || item.LessonId == notMp4.LessonId || item.LessonId == notVideo.LessonId),
                Is.EqualTo(0));
        });
    }

    // Traces to: L2-CURRIC-002 (AC5 permission matrix)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Only_a_program_administrator_may_complete_an_upload()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, lessonId, session) = await UploadAllChunksAsync(adminToken, FakeMp4((int)(2 * MiB)));
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var youth = await CompleteAsync(youthToken, lessonId, session.UploadId);
        var anonymous = await CompleteAsync(null, lessonId, session.UploadId);

        Assert.Multiple(() =>
        {
            Assert.That(youth.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

}

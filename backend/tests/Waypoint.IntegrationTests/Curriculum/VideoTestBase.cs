using System.Net;
using System.Net.Http.Json;

namespace Waypoint.IntegrationTests.Curriculum;

/// <summary>Shared helpers for the lesson video acceptance tests.</summary>
public abstract class VideoTestBase : IntegrationTestBase
{
    protected const long MiB = 1024 * 1024;

    protected async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string url, string? accessToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        if (accessToken is not null) Authorize(request, accessToken);
        return await Client.SendAsync(request);
    }

    protected async Task<Guid> CreateLessonAsync(string adminToken)
    {
        var trackId = (await (await SendAsync(HttpMethod.Post, "/api/tracks", adminToken,
            new { name = "Video Track", key = $"video-{Guid.NewGuid():N}" }))
            .Content.ReadFromJsonAsync<IdDto>())!.Id;
        var moduleId = (await (await SendAsync(HttpMethod.Post, "/api/modules", adminToken,
            new { trackId, name = "Video Module" })).Content.ReadFromJsonAsync<IdDto>())!.Id;
        return (await (await SendAsync(HttpMethod.Post, "/api/lessons", adminToken,
            new { moduleId, title = "Video Lesson" })).Content.ReadFromJsonAsync<IdDto>())!.Id;
    }

    protected async Task<UploadSessionDto> StartAsync(string adminToken, Guid lessonId, long sizeBytes)
    {
        var response = await SendAsync(HttpMethod.Post, $"/api/lessons/{lessonId}/video-uploads",
            adminToken, new { fileName = "week-1.mp4", sizeBytes });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        return (await response.Content.ReadFromJsonAsync<UploadSessionDto>())!;
    }

    protected async Task<HttpResponseMessage> PutChunkAsync(
        string? accessToken, Guid lessonId, Guid uploadId, int index, int length)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Put, $"/api/lessons/{lessonId}/video-uploads/{uploadId}/chunks/{index}")
        {
            Content = new ByteArrayContent(new byte[length]),
        };
        request.Content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        if (accessToken is not null) Authorize(request, accessToken);
        return await Client.SendAsync(request);
    }

    protected async Task<StatusDto> StatusAsync(string adminToken, Guid lessonId, Guid uploadId)
    {
        var response = await SendAsync(
            HttpMethod.Get, $"/api/lessons/{lessonId}/video-uploads/{uploadId}", adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return (await response.Content.ReadFromJsonAsync<StatusDto>())!;
    }

    protected static byte[] FakeMp4(int length, string marker = "")
    {
        var bytes = new byte[length];
        "\0\0\0\u0018ftypisom"u8.CopyTo(bytes);
        System.Text.Encoding.ASCII.GetBytes(marker).CopyTo(bytes, 16);
        return bytes;
    }

    protected async Task<(Guid TrackId, Guid LessonId, UploadSessionDto Session)> UploadAllChunksAsync(
        string adminToken, byte[] content, int skipIndex = -1)
    {
        var trackId = Guid.Empty;
        var lessonId = await CreateLessonAsync(adminToken);
        var session = await StartAsync(adminToken, lessonId, content.Length);
        for (var index = 0; index < session.ChunkCount; index++)
        {
            if (index == skipIndex) continue;
            var offset = index * session.ChunkSizeBytes;
            var length = Math.Min(session.ChunkSizeBytes, content.Length - offset);
            var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"/api/lessons/{lessonId}/video-uploads/{session.UploadId}/chunks/{index}")
            {
                Content = new ByteArrayContent(content, offset, length),
            };
            request.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            Authorize(request, adminToken);
            Assert.That((await Client.SendAsync(request)).StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }
        return (trackId, lessonId, session);
    }

    protected Task<HttpResponseMessage> CompleteAsync(string? token, Guid lessonId, Guid uploadId) =>
        SendAsync(HttpMethod.Post, $"/api/lessons/{lessonId}/video-uploads/{uploadId}/complete", token);

    protected sealed record CompletedMediaDto(Guid Id, string MediaType);
    protected sealed record IdDto(Guid Id);
    protected sealed record UploadSessionDto(Guid UploadId, int ChunkSizeBytes, int ChunkCount);
    protected sealed record StatusDto(Guid UploadId, int ChunkCount, int[] ReceivedChunks);
}

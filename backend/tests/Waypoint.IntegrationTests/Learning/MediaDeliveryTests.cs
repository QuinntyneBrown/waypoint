using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;
using Waypoint.IntegrationTests.Curriculum;

namespace Waypoint.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-002 (AC5 seekable adaptive stream, AC6 only a valid signed address is served),
//            L2-NFR-001 (AC4 media served without script exposure)
[TestFixture]
[Property("Req", "L2-LEARN-002")]
public sealed class MediaDeliveryTests : VideoTestBase
{
    private sealed record PlaybackDto(string Url, string PosterUrl);

    private Guid _mediaId;
    private string _baseUrl = string.Empty;

    [OneTimeSetUp]
    public async Task PublishAReadyVideoAsync()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, lessonId, session) = await UploadAllChunksAsync(adminToken, FakeMp4((int)(2 * MiB)));
        var completed = await CompleteAsync(adminToken, lessonId, session.UploadId);
        _mediaId = (await completed.Content.ReadFromJsonAsync<IdDto>())!.Id;
        using (var scope = Factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<VideoTranscodeProcessor>().RunOnceAsync();
        }
        var playback = await (await SendAsync(
            HttpMethod.Get, $"/api/lesson-media/{_mediaId}/playback", adminToken))
            .Content.ReadFromJsonAsync<PlaybackDto>();
        _baseUrl = playback!.Url[..playback.Url.LastIndexOf('/')];
    }

    private Task<HttpResponseMessage> GetAsync(string url, Action<HttpRequestMessage>? configure = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        configure?.Invoke(request);
        return Client.SendAsync(request);
    }

    // Traces to: L2-LEARN-002 (AC5 master and variant playlists), L2-NFR-001 (AC4 safe serving)
    [Test]
    [Property("Req", "L2-LEARN-002")]
    [Property("Req", "L2-NFR-001")]
    public async Task Playlists_are_served_without_a_sign_in_and_stay_short_lived()
    {
        var master = await GetAsync($"{_baseUrl}/master.m3u8");
        var variant = await GetAsync($"{_baseUrl}/360p/index.m3u8");

        Assert.Multiple(() =>
        {
            Assert.That(master.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(master.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/vnd.apple.mpegurl"));
            Assert.That(master.Headers.CacheControl?.ToString(), Is.EqualTo("max-age=300, private"));
            Assert.That(master.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
            Assert.That(master.Headers.ETag, Is.Not.Null);
            Assert.That(master.Content.ReadAsStringAsync().Result, Does.Contain("360p/index.m3u8"));
            Assert.That(variant.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(variant.Content.ReadAsStringAsync().Result, Does.Contain("seg_00001.m4s"));
        });
    }

    // Traces to: L2-LEARN-002 (AC5 seeking needs byte ranges and immutable segments)
    [Test]
    [Property("Req", "L2-LEARN-002")]
    public async Task Segments_support_ranges_and_conditional_requests_and_are_cached_as_immutable()
    {
        var full = await GetAsync($"{_baseUrl}/360p/seg_00001.m4s");
        var etag = full.Headers.ETag!;
        var range = await GetAsync(
            $"{_baseUrl}/360p/seg_00001.m4s",
            request => request.Headers.Range = new RangeHeaderValue(100, 199));
        var conditional = await GetAsync(
            $"{_baseUrl}/360p/seg_00001.m4s",
            request => request.Headers.IfNoneMatch.Add(etag));
        var init = await GetAsync($"{_baseUrl}/360p/init.mp4");
        var poster = await GetAsync($"{_baseUrl}/poster.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(full.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(full.Content.Headers.ContentType?.MediaType, Is.EqualTo("video/iso.segment"));
            Assert.That(full.Content.Headers.ContentLength, Is.EqualTo(4096));
            Assert.That(full.Headers.CacheControl?.ToString(), Is.EqualTo("max-age=31536000, private, immutable"));
            Assert.That(range.StatusCode, Is.EqualTo(HttpStatusCode.PartialContent));
            Assert.That(range.Content.Headers.ContentRange?.ToString(), Is.EqualTo("bytes 100-199/4096"));
            Assert.That(
                range.Content.ReadAsByteArrayAsync().Result,
                Is.EqualTo(Enumerable.Range(100, 100).Select(n => (byte)(n % 251)).ToArray()));
            Assert.That(conditional.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
            Assert.That(init.Content.Headers.ContentType?.MediaType, Is.EqualTo("video/mp4"));
            Assert.That(poster.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/jpeg"));
        });
    }

    // Traces to: L2-LEARN-002 (AC5 playlists are compressed, segments are not)
    [Test]
    [Property("Req", "L2-LEARN-002")]
    public async Task Playlists_are_compressed_when_requested_but_segments_are_not()
    {
        void AcceptGzip(HttpRequestMessage request) => request.Headers.AcceptEncoding.ParseAdd("gzip");

        var playlist = await GetAsync($"{_baseUrl}/master.m3u8", AcceptGzip);
        var segment = await GetAsync($"{_baseUrl}/360p/seg_00001.m4s", AcceptGzip);

        Assert.Multiple(() =>
        {
            Assert.That(playlist.Content.Headers.ContentEncoding, Does.Contain("gzip"));
            Assert.That(segment.Content.Headers.ContentEncoding, Is.Empty);
        });
    }

    // Traces to: L2-LEARN-002 (AC6 direct requests without a valid signature are rejected),
    //            L2-NFR-001 (AC4 no path escape)
    [Test]
    [Property("Req", "L2-LEARN-002")]
    [Property("Req", "L2-NFR-001")]
    public async Task Requests_with_a_bad_token_another_videos_token_or_an_unsafe_path_are_not_found()
    {
        var token = _baseUrl.Split('/')[3];
        var tampered = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');
        var expiredSigner = new HmacPlaybackUrlSigner(
            Options.Create(new MediaOptions
            {
                PlaybackSigningKey = Factory.PlaybackSigningKey,
                PlaybackTokenLifetime = TimeSpan.FromMinutes(-1),
            }),
            new SystemClock());
        var expired = expiredSigner.Sign(_mediaId).Token;
        var otherMedia = Guid.NewGuid();
        string Url(string tokenValue, Guid media, string path) => $"/api/media/{tokenValue}/{media}/{path}";

        var responses = new[]
        {
            await GetAsync(Url(tampered, _mediaId, "master.m3u8")),
            await GetAsync(Url("not-a-token", _mediaId, "master.m3u8")),
            await GetAsync(Url(expired, _mediaId, "master.m3u8")),
            await GetAsync(Url(token, otherMedia, "master.m3u8")),
            await GetAsync(Url(token, _mediaId, "360p/..%2f..%2f..%2fsources%2f" + _mediaId.ToString("N") + ".mp4")),
            await GetAsync(Url(token, _mediaId, "360p/missing.m4s")),
            await GetAsync(Url(token, _mediaId, "360p/notes.txt")),
        };

        Assert.That(responses.Select(r => r.StatusCode), Is.All.EqualTo(HttpStatusCode.NotFound));
    }
}

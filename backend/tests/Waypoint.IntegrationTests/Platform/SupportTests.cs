// Acceptance Test
// Traces to: L2-PLATFORM-005
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-005")]
public sealed class SupportTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        if (token is not null)
        {
            Authorize(request, token);
        }
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> UploadScreenshotAsync(
        string token,
        string fileName,
        string contentType,
        byte[] content)
    {
        var form = new MultipartFormDataContent();
        var screenshot = new ByteArrayContent(content);
        screenshot.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(screenshot, "file", fileName);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/support/screenshots")
        {
            Content = form,
        };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    [Test]
    [Property("Req", "L2-PLATFORM-005")]
    public async Task Submitting_a_support_request_persists_it_and_emails_the_director()
    {
        // Traces to: L2-PLATFORM-005
        const string director = "director@waypoint.local";
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.OrgSettings.Add(new OrgSettings { Name = "Waypoint", ContactEmail = director });
            await db.SaveChangesAsync();
        }

        var token = await CreateUserAndGetTokenAsync(Roles.Youth);

        var response = await SendAsync(HttpMethod.Post, "/api/support", body: new
        {
            category = "Bug",
            description = "The lesson video will not play on my phone.",
            replyToEmail = "youth.reply@example.com",
        }, token: token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<SupportRequestResultDto>();
        Assert.That(result!.Id, Is.Not.EqualTo(Guid.Empty));

        // The request is persisted.
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = db.SupportRequests.Single(s => s.Id == result.Id);
            Assert.That(stored.Category, Is.EqualTo("Bug"));
            Assert.That(stored.ReplyToEmail, Is.EqualTo("youth.reply@example.com"));
        }

        // An email was routed to the configured director inbox.
        var sent = Factory.Emails.LastTo(director);
        Assert.That(sent, Is.Not.Null, "a support email should be sent to the director inbox");
    }

    [Test]
    [Property("Req", "L2-PLATFORM-005")]
    [Property("Req", "L2-NFR-001")]
    public async Task Screenshot_upload_is_sniffed_removable_request_linked_and_privately_served()
    {
        // Traces to: L2-PLATFORM-005 AC2, L2-NFR-001 AC4
        var ownerToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var otherToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var administratorToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var disguised = await UploadScreenshotAsync(
            ownerToken,
            "problem.png",
            "image/png",
            "<script>alert(1)</script>"u8.ToArray());
        Assert.That(disguised.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        var removableResponse = await UploadScreenshotAsync(
            ownerToken, "remove-me.png", "image/png", png);
        Assert.That(removableResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var removable = await removableResponse.Content.ReadFromJsonAsync<ScreenshotDto>();
        var removed = await SendAsync(HttpMethod.Delete, removable!.Url, token: ownerToken);
        Assert.That(removed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await SendAsync(HttpMethod.Get, removable.Url, token: ownerToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));

        var unusedResponse = await UploadScreenshotAsync(
            ownerToken, "unused.png", "image/png", png);
        var selectedResponse = await UploadScreenshotAsync(
            ownerToken, "problem.png", "image/png", png);
        Assert.That(selectedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var unused = await unusedResponse.Content.ReadFromJsonAsync<ScreenshotDto>();
        var selected = await selectedResponse.Content.ReadFromJsonAsync<ScreenshotDto>();
        Assert.That((await SendAsync(HttpMethod.Get, selected!.Url, token: administratorToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));

        var submitted = await SendAsync(HttpMethod.Post, "/api/support", body: new
        {
            category = "A technical problem",
            description = "The lesson page fails to load; see the attached screenshot.",
            replyToEmail = "owner@example.com",
            screenshotFileId = selected.Id,
        }, token: ownerToken);
        Assert.That(submitted.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var ownerDownload = await SendAsync(HttpMethod.Get, selected.Url, token: ownerToken);
        Assert.Multiple(() =>
        {
            Assert.That(ownerDownload.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(ownerDownload.Content.Headers.ContentType!.MediaType, Is.EqualTo("image/png"));
            Assert.That(ownerDownload.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
        });
        Assert.That(await ownerDownload.Content.ReadAsByteArrayAsync(), Is.EqualTo(png));
        Assert.That((await SendAsync(HttpMethod.Get, unused!.Url, token: ownerToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That((await SendAsync(HttpMethod.Get, selected.Url, token: otherToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That((await SendAsync(HttpMethod.Get, selected.Url, token: administratorToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await Client.GetAsync(selected.Url)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That((await SendAsync(HttpMethod.Delete, selected.Url, token: ownerToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.Conflict));
    }

    private sealed record SupportRequestResultDto(Guid Id);
    private sealed record ScreenshotDto(
        Guid Id,
        string FileName,
        string ContentType,
        long SizeBytes,
        string Url);
}

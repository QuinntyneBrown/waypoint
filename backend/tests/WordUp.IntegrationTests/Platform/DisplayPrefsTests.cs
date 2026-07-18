// Acceptance Test
// Traces to: L2-PLATFORM-004
using System.Net;
using System.Net.Http.Json;
using WordUp.Domain;

namespace WordUp.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-004")]
public sealed class DisplayPrefsTests : IntegrationTestBase
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

    [Test]
    [Property("Req", "L2-PLATFORM-004")]
    public async Task Saving_display_preferences_is_reflected_on_read()
    {
        // Traces to: L2-PLATFORM-004
        var token = await CreateUserAndGetTokenAsync(Roles.Youth);

        var put = await SendAsync(HttpMethod.Put, "/api/me/display-preferences", body: new
        {
            textScalePercent = 150,
            highContrast = true,
            reduceMotion = false,
            underlineLinks = true,
            readingFont = "OpenDyslexic",
        }, token: token);
        Assert.That(put.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var get = await SendAsync(HttpMethod.Get, "/api/me/display-preferences", token: token);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var view = await get.Content.ReadFromJsonAsync<DisplayPreferenceDto>();
        Assert.That(view!.TextScalePercent, Is.EqualTo(150));
        Assert.That(view.HighContrast, Is.True);
        Assert.That(view.ReduceMotion, Is.False);
        Assert.That(view.UnderlineLinks, Is.True);
        Assert.That(view.ReadingFont, Is.EqualTo("OpenDyslexic"));
    }

    [Test]
    [Property("Req", "L2-PLATFORM-004")]
    public async Task A_text_scale_outside_the_allowed_range_is_rejected()
    {
        // Traces to: L2-PLATFORM-004
        var token = await CreateUserAndGetTokenAsync(Roles.Youth);

        var put = await SendAsync(HttpMethod.Put, "/api/me/display-preferences", body: new
        {
            textScalePercent = 400,
            highContrast = false,
            reduceMotion = false,
            underlineLinks = false,
        }, token: token);
        Assert.That(put.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private sealed record DisplayPreferenceDto(
        int TextScalePercent, bool HighContrast, bool ReduceMotion, bool UnderlineLinks, string? ReadingFont);
}

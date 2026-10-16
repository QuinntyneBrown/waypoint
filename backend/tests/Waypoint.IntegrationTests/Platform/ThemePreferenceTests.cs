// Acceptance Test
// Traces to: L2-PLATFORM-013
using System.Net;
using System.Net.Http.Json;

namespace Waypoint.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-013")]
public sealed class ThemePreferenceTests : IntegrationTestBase
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

    private static object Preferences(string? theme) => new
    {
        textScalePercent = 100,
        highContrast = false,
        reduceMotion = false,
        underlineLinks = false,
        theme,
    };

    private async Task<string> ReadThemeAsync(string token)
    {
        var get = await SendAsync(HttpMethod.Get, "/api/me/display-preferences", token: token);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var view = await get.Content.ReadFromJsonAsync<ThemeDto>();
        return view!.Theme;
    }

    [Test]
    [Property("Req", "L2-PLATFORM-013")]
    public async Task A_user_who_never_chose_a_theme_follows_the_system()
    {
        // Traces to: L2-PLATFORM-013 AC1
        var token = await CreateUserAndGetTokenAsync(Roles.Youth);

        Assert.That(await ReadThemeAsync(token), Is.EqualTo("system"));
    }

    [Test]
    [Property("Req", "L2-PLATFORM-013")]
    public async Task A_saved_theme_is_returned_on_later_reads()
    {
        // Traces to: L2-PLATFORM-013 AC3
        var token = await CreateUserAndGetTokenAsync(Roles.Mentor);

        var put = await SendAsync(HttpMethod.Put, "/api/me/display-preferences", Preferences("dark"), token);

        Assert.That(put.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await ReadThemeAsync(token), Is.EqualTo("dark"));
    }

    [Test]
    [Property("Req", "L2-PLATFORM-013")]
    public async Task An_unknown_theme_is_rejected_and_nothing_is_stored()
    {
        // Traces to: L2-PLATFORM-013 AC4
        var token = await CreateUserAndGetTokenAsync(Roles.Youth);
        await SendAsync(HttpMethod.Put, "/api/me/display-preferences", Preferences("light"), token);

        var put = await SendAsync(HttpMethod.Put, "/api/me/display-preferences", Preferences("purple"), token);

        Assert.That(put.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await ReadThemeAsync(token), Is.EqualTo("light"));
    }

    private sealed record ThemeDto(string Theme);
}

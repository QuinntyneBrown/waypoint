// Acceptance Test
// Traces to: L2-PLATFORM-003
using System.Net.Http.Json;

namespace Waypoint.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-003")]
public sealed class NotificationPrefsTests : IntegrationTestBase
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
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Saving_a_notification_preference_persists_and_a_second_save_updates_it_in_place()
    {
        // Traces to: L2-PLATFORM-003
        var token = await CreateUserAndGetTokenAsync(Roles.Youth);

        // Turn both channels off for the canonical mentor-message event.
        var put1 = await SendAsync(HttpMethod.Put, "/api/me/notification-preferences", body: new
        {
            eventKey = "mentor-messages",
            inApp = false,
            email = false,
        }, token: token);
        Assert.That(put1.IsSuccessStatusCode, Is.True);

        var get1 = await SendAsync(HttpMethod.Get, "/api/me/notification-preferences", token: token);
        var prefs1 = await get1.Content.ReadFromJsonAsync<List<NotificationPreferenceDto>>();
        var entry = prefs1!.Single(p => p.EventKey == "mentor-messages");
        Assert.That(entry.InApp, Is.False);
        Assert.That(entry.Email, Is.False);

        // Saving the same event again updates in place (no duplicate row).
        var put2 = await SendAsync(HttpMethod.Put, "/api/me/notification-preferences", body: new
        {
            eventKey = "mentor-messages",
            inApp = true,
            email = true,
        }, token: token);
        Assert.That(put2.IsSuccessStatusCode, Is.True);

        var get2 = await SendAsync(HttpMethod.Get, "/api/me/notification-preferences", token: token);
        var prefs2 = await get2.Content.ReadFromJsonAsync<List<NotificationPreferenceDto>>();
        var matches = prefs2!.Where(p => p.EventKey == "mentor-messages").ToList();
        Assert.That(matches.Count, Is.EqualTo(1));
        Assert.That(matches[0].InApp, Is.True);
        Assert.That(matches[0].Email, Is.True);
    }

    private sealed record NotificationPreferenceDto(string EventKey, bool InApp, bool Email);
}

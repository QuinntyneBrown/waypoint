using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Engagement;

// Acceptance Test
// Traces to: L2-ENGAGE-008 (in-app notification feed: mentor-message notifications, mark-all-read, preference opt-out)
[TestFixture]
[Property("Req", "L2-ENGAGE-008")]
public sealed class NotificationTests : IntegrationTestBase
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

    private async Task<(Guid Id, string Email)> SeedUserAsync(string role, Guid? mentorId = null)
    {
        var email = $"seed.{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = email,
            DisplayName = $"Seed {role}",
            Role = role,
            Status = UserStatus.Active,
            MentorId = mentorId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task SeedNotificationPreferenceAsync(
        Guid userId,
        string eventKey,
        bool inApp,
        bool email = false)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = userId,
            EventKey = eventKey,
            InApp = inApp,
            Email = email,
        });
        await db.SaveChangesAsync();
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-ENGAGE-008")]
    public async Task A_new_message_notifies_the_recipient_and_mark_all_read_clears_it()
    {
        // Traces to: L2-ENGAGE-008
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);
        var youthToken = await SignInAsync(youth.Email);

        var send = await SendAsync(HttpMethod.Post, "/api/messages", body: new { toUserId = youth.Id, body = "Checking in!" }, token: mentorToken);
        Assert.That(send.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var feed = await SendAsync(HttpMethod.Get, "/api/notifications", token: youthToken);
        Assert.That(feed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var notifications = await feed.Content.ReadFromJsonAsync<List<NotificationDto>>();
        var newMessage = notifications!.SingleOrDefault(n => n.EventKey == "mentor-messages");
        Assert.That(newMessage, Is.Not.Null, "recipient should have a mentor-message notification");
        Assert.That(newMessage!.Read, Is.False);
        Assert.That(newMessage.LinkPath, Is.Not.Null.And.Not.Empty);

        var markAll = await SendAsync(HttpMethod.Post, "/api/notifications/read-all", token: youthToken);
        Assert.That(markAll.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var feed2 = await SendAsync(HttpMethod.Get, "/api/notifications", token: youthToken);
        var notifications2 = await feed2.Content.ReadFromJsonAsync<List<NotificationDto>>();
        Assert.That(notifications2!.Single(n => n.EventKey == "mentor-messages").Read, Is.True);
    }

    [Test]
    [Property("Req", "L2-ENGAGE-008")]
    public async Task No_notification_is_created_when_the_recipient_opted_out_of_in_app_new_message()
    {
        // Traces to: L2-ENGAGE-008 (AC4)
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        await SeedNotificationPreferenceAsync(youth.Id, "mentor-messages", inApp: false);
        var mentorToken = await SignInAsync(mentor.Email);
        var youthToken = await SignInAsync(youth.Email);

        var send = await SendAsync(HttpMethod.Post, "/api/messages", body: new { toUserId = youth.Id, body = "You won't get pinged." }, token: mentorToken);
        Assert.That(send.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var feed = await SendAsync(HttpMethod.Get, "/api/notifications", token: youthToken);
        var notifications = await feed.Content.ReadFromJsonAsync<List<NotificationDto>>();
        Assert.That(notifications!.Any(n => n.EventKey == "mentor-messages"), Is.False, "opting out of in-app mentor messages suppresses the notification");
    }

    [Test]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task A_new_message_uses_email_when_enabled_and_in_app_is_disabled()
    {
        // Traces to: L2-ENGAGE-008 AC1/AC4, L2-PLATFORM-003 AC2
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        await SeedNotificationPreferenceAsync(
            youth.Id,
            "mentor-messages",
            inApp: false,
            email: true);
        var mentorToken = await SignInAsync(mentor.Email);
        var youthToken = await SignInAsync(youth.Email);

        var send = await SendAsync(
            HttpMethod.Post,
            "/api/messages",
            body: new { toUserId = youth.Id, body = "Email this safely." },
            token: mentorToken);
        Assert.That(send.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var feed = await SendAsync(HttpMethod.Get, "/api/notifications", token: youthToken);
        var notifications = await feed.Content.ReadFromJsonAsync<List<NotificationDto>>();
        Assert.That(notifications!.Any(item => item.EventKey == "mentor-messages"), Is.False);
        var email = Factory.Emails.LastTo(youth.Email);
        Assert.Multiple(() =>
        {
            Assert.That(email, Is.Not.Null);
            Assert.That(email!.Subject, Does.Contain("new message").IgnoreCase);
            Assert.That(email.Body, Does.Contain("/youth/messages"));
        });
    }

    private sealed record NotificationDto(
        Guid Id, string EventKey, string Title, string Body, string? LinkPath, bool Read, DateTime CreatedAt);
}

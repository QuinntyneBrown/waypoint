using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Engagement.Messaging;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Engagement;

// Acceptance Tests
// Traces to: L2-ENGAGE-007, L2-ENGAGE-008, L2-NFR-005, L2-AUTH-010
[TestFixture]
[Property("Req", "L2-ENGAGE-007")]
public sealed class MentorMessagingTests : IntegrationTestBase
{
    [Test]
    public async Task Mentor_inbox_has_scoped_threads_unread_state_and_a_role_correct_notification()
    {
        var setup = await SeedAsync();
        var mentorToken = await SignInAsync(setup.MentorEmail);
        var youthToken = await SignInAsync(setup.YouthOneEmail);
        var outsiderToken = await SignInAsync(setup.OtherMentorEmail);

        var send = await SendAsync(
            HttpMethod.Post,
            "/api/messages",
            new { toUserId = setup.MentorId, body = "<strong>Hello mentor</strong>" },
            youthToken);
        Assert.That(send.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var inboxResponse = await SendAsync(HttpMethod.Get, "/api/mentor/messages", null, mentorToken);
        Assert.That(inboxResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var inbox = await inboxResponse.Content.ReadFromJsonAsync<MentorMessageInboxView>();
        Assert.That(inbox!.Conversations.Select(item => item.YouthId), Is.EquivalentTo(
            new[] { setup.YouthOneId, setup.YouthTwoId }));
        var conversation = inbox.Conversations.Single(item => item.YouthId == setup.YouthOneId);
        Assert.Multiple(() =>
        {
            Assert.That(conversation.CohortName, Is.EqualTo("Saturday Leaders"));
            Assert.That(conversation.UnreadCount, Is.EqualTo(1));
            Assert.That(conversation.LastMessagePreview, Is.EqualTo("<strong>Hello mentor</strong>"));
            Assert.That(inbox.Groups.Single().RecipientCount, Is.EqualTo(2));
        });

        var feed = await SendAsync(HttpMethod.Get, "/api/notifications", null, mentorToken);
        var notifications = await feed.Content.ReadFromJsonAsync<List<NotificationView>>();
        Assert.That(notifications!.Single().LinkPath, Is.EqualTo("/mentor/messages"));

        var thread = await SendAsync(
            HttpMethod.Get,
            $"/api/messages/{setup.YouthOneId}",
            null,
            mentorToken);
        Assert.That(thread.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var messages = await thread.Content.ReadFromJsonAsync<List<MessageView>>();
        Assert.That(messages!.Single().Body, Is.EqualTo("<strong>Hello mentor</strong>"));

        var refreshed = await SendAsync(HttpMethod.Get, "/api/mentor/messages", null, mentorToken);
        var refreshedInbox = await refreshed.Content.ReadFromJsonAsync<MentorMessageInboxView>();
        Assert.That(
            refreshedInbox!.Conversations.Single(item => item.YouthId == setup.YouthOneId).UnreadCount,
            Is.Zero);

        var forbidden = await SendAsync(
            HttpMethod.Get,
            $"/api/messages/{setup.YouthOneId}",
            null,
            outsiderToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Cohort_broadcast_fans_out_private_messages_live_notifications_and_an_audit()
    {
        var setup = await SeedAsync(optOutYouthTwo: true);
        var mentorToken = await SignInAsync(setup.MentorEmail);
        var otherMentorToken = await SignInAsync(setup.OtherMentorEmail);
        const string body = "<script>alert('no')</script> Great work!";

        var forbidden = await SendAsync(
            HttpMethod.Post,
            "/api/messages/broadcast",
            new { cohortId = setup.CohortId, body },
            otherMentorToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var response = await SendAsync(
            HttpMethod.Post,
            "/api/messages/broadcast",
            new { cohortId = setup.CohortId, body },
            mentorToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<BroadcastResult>();
        Assert.That(result!.Sent, Is.EqualTo(2));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var broadcastMessages = await db.Messages
            .Where(message => message.SenderId == setup.MentorId)
            .ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(
                broadcastMessages.Select(message => message.RecipientId),
                Is.EquivalentTo(new[] { setup.YouthOneId, setup.YouthTwoId }));
            Assert.That(broadcastMessages.All(message => !message.Body.Contains("<script>")), Is.True);
            Assert.That(
                db.Notifications.Count(item => item.EventKey == "mentor-messages"),
                Is.EqualTo(1),
                "the opted-out youth still receives the message but no in-app notification");
            Assert.That(
                db.AuditEntries.Any(entry =>
                    entry.Action == "MentorMessageBroadcast"
                    && entry.TargetId == setup.CohortId
                    && entry.Details == "Recipients=2"),
                Is.True);
        });
    }

    private async Task<SeedResult> SeedAsync(bool optOutYouthTwo = false)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var suffix = Guid.NewGuid().ToString("N");
        var mentor = UserFor(Roles.Mentor, $"mentor.{suffix}@example.com", "Mentor Casey", hasher);
        var otherMentor = UserFor(
            Roles.Mentor,
            $"other.mentor.{suffix}@example.com",
            "Mentor Outside",
            hasher);
        var cohort = new Cohort
        {
            Name = "Saturday Leaders",
            MentorId = mentor.Id,
            TermStart = DateTime.UtcNow.Date.AddMonths(-1),
            TermEnd = DateTime.UtcNow.Date.AddMonths(6),
        };
        var youthOne = UserFor(
            Roles.Youth,
            $"youth.one.{suffix}@example.com",
            "Alex Youth",
            hasher,
            mentor.Id,
            cohort.Id);
        var youthTwo = UserFor(
            Roles.Youth,
            $"youth.two.{suffix}@example.com",
            "Bailey Youth",
            hasher,
            mentor.Id,
            cohort.Id);
        var inactive = UserFor(
            Roles.Youth,
            $"inactive.{suffix}@example.com",
            "Inactive Youth",
            hasher,
            mentor.Id,
            cohort.Id);
        inactive.Status = UserStatus.Deactivated;
        var outsider = UserFor(
            Roles.Youth,
            $"outsider.{suffix}@example.com",
            "Outside Youth",
            hasher,
            otherMentor.Id);
        db.AddRange(mentor, otherMentor, cohort, youthOne, youthTwo, inactive, outsider);
        if (optOutYouthTwo)
        {
            db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = youthTwo.Id,
                EventKey = "mentor-messages",
                InApp = false,
                Email = false,
            });
        }
        await db.SaveChangesAsync();
        return new SeedResult(
            mentor.Id,
            mentor.Email,
            otherMentor.Email,
            youthOne.Id,
            youthOne.Email,
            youthTwo.Id,
            cohort.Id);
    }

    private static User UserFor(
        string role,
        string email,
        string name,
        IPasswordHasher hasher,
        Guid? mentorId = null,
        Guid? cohortId = null) =>
        new()
        {
            Email = email,
            DisplayName = name,
            Role = role,
            Status = UserStatus.Active,
            MentorId = mentorId,
            CohortId = cohortId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResult>())!.AccessToken;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body,
        string token)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private sealed record SeedResult(
        Guid MentorId,
        string MentorEmail,
        string OtherMentorEmail,
        Guid YouthOneId,
        string YouthOneEmail,
        Guid YouthTwoId,
        Guid CohortId);
}

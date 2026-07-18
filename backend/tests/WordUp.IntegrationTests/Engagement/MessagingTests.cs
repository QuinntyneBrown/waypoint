using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Engagement;

// Acceptance Test
// Traces to: L2-ENGAGE-007 (1:1 messaging within the mentoring relationship; broadcast; thread access control),
//            L2-AUTH-010 (no-IDOR: only participants may read a thread),
//            L2-NFR-001 (stored message bodies are HTML-encoded to neutralise stored XSS)
[TestFixture]
[Property("Req", "L2-ENGAGE-007")]
public sealed class MessagingTests : IntegrationTestBase
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

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-ENGAGE-007")]
    public async Task Mentor_and_assigned_youth_exchange_messages_in_a_single_thread()
    {
        // Traces to: L2-ENGAGE-007
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);
        var youthToken = await SignInAsync(youth.Email);

        var first = await SendAsync(HttpMethod.Post, "/api/messages", body: new { toUserId = youth.Id, body = "Hi, how did the reading go?" }, token: mentorToken);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var reply = await SendAsync(HttpMethod.Post, "/api/messages", body: new { toUserId = mentor.Id, body = "Finished it, loved chapter 3." }, token: youthToken);
        Assert.That(reply.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Both participants see the same single thread with both messages.
        var mentorThread = await SendAsync(HttpMethod.Get, $"/api/messages/{youth.Id}", token: mentorToken);
        Assert.That(mentorThread.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var mentorMessages = await mentorThread.Content.ReadFromJsonAsync<List<MessageDto>>();
        Assert.That(mentorMessages!.Count, Is.EqualTo(2));
        Assert.That(mentorMessages.Any(m => m.SenderId == mentor.Id && m.Body == "Hi, how did the reading go?"), Is.True);
        Assert.That(mentorMessages.Any(m => m.SenderId == youth.Id && m.Body == "Finished it, loved chapter 3."), Is.True);

        var youthThread = await SendAsync(HttpMethod.Get, $"/api/messages/{mentor.Id}", token: youthToken);
        var youthMessages = await youthThread.Content.ReadFromJsonAsync<List<MessageDto>>();
        Assert.That(youthMessages!.Count, Is.EqualTo(2), "the reply lands in the same thread, not a new one");
    }

    [Test]
    [Property("Req", "L2-ENGAGE-007")]
    [Property("Req", "L2-NFR-001")]
    public async Task A_message_body_is_stored_html_encoded()
    {
        // Traces to: L2-ENGAGE-007, L2-NFR-001
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);

        const string payload = "<script>alert(1)</script>";
        var send = await SendAsync(HttpMethod.Post, "/api/messages", body: new { toUserId = youth.Id, body = payload }, token: mentorToken);
        Assert.That(send.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await send.Content.ReadFromJsonAsync<MessageResultDto>();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = db.Messages.Single(m => m.Id == result!.Id);
        Assert.That(stored.Body, Does.Not.Contain("<script>"), "the raw script tag must not be persisted");
        Assert.That(stored.Body, Does.Contain("&lt;script&gt;"));
    }

    [Test]
    [Property("Req", "L2-ENGAGE-007")]
    public async Task A_youth_cannot_message_someone_who_is_not_their_mentor()
    {
        // Traces to: L2-ENGAGE-007 (AC4 / L2-AUTH-010)
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var otherMentor = await SeedUserAsync(Roles.Mentor);
        var youthToken = await SignInAsync(youth.Email);

        var send = await SendAsync(HttpMethod.Post, "/api/messages", body: new { toUserId = otherMentor.Id, body = "Hello?" }, token: youthToken);
        Assert.That(send.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-ENGAGE-007")]
    public async Task A_third_party_cannot_read_a_thread_they_are_not_part_of()
    {
        // Traces to: L2-ENGAGE-007 (AC4 / L2-AUTH-010)
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);

        await SendAsync(HttpMethod.Post, "/api/messages", body: new { toUserId = youth.Id, body = "Private note." }, token: mentorToken);

        // An unrelated youth (assigned to a different mentor) tries to read the mentor's thread.
        var otherMentor = await SeedUserAsync(Roles.Mentor);
        var outsider = await SeedUserAsync(Roles.Youth, mentorId: otherMentor.Id);
        var outsiderToken = await SignInAsync(outsider.Email);

        var peek = await SendAsync(HttpMethod.Get, $"/api/messages/{mentor.Id}", token: outsiderToken);
        Assert.That(peek.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private sealed record MessageResultDto(Guid Id);

    private sealed record MessageDto(Guid Id, Guid SenderId, Guid RecipientId, string Body, DateTime SentAt, DateTime? ReadAt);
}

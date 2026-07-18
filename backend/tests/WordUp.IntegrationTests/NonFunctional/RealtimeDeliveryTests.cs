// Acceptance Test
// Traces to: L2-NFR-005 (near-real-time delivery of notifications/messages to the correctly-scoped recipient)
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-005")]
public sealed class RealtimeDeliveryTests : IntegrationTestBase
{
    private async Task<(Guid Id, string Token)> SeedAndSignInAsync(string role, Guid? mentorId = null)
    {
        var email = $"{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        const string password = "Passw0rd!";
        Guid id;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = new User
            {
                Email = email,
                DisplayName = role,
                Role = role,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(password),
                MentorId = mentorId,
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            id = user.Id;
        }

        var resp = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        resp.EnsureSuccessStatusCode();
        var auth = await resp.Content.ReadFromJsonAsync<AuthDto>();
        return (id, auth!.AccessToken);
    }

    private HubConnection BuildHubConnection(string token) =>
        new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/realtime", options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .Build();

    [Test]
    [Property("Req", "L2-NFR-005")]
    public async Task A_new_message_is_pushed_in_near_real_time_to_the_recipient_only()
    {
        // Traces to: L2-NFR-005 AC1/AC4
        var mentor = await SeedAndSignInAsync(Roles.Mentor);
        var youth = await SeedAndSignInAsync(Roles.Youth, mentorId: mentor.Id);
        var bystander = await SeedAndSignInAsync(Roles.Youth, mentorId: mentor.Id);

        var youthConn = BuildHubConnection(youth.Token);
        var bystanderConn = BuildHubConnection(bystander.Token);

        var youthGot = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bystanderGot = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        youthConn.On<JsonElement>("notification", p => youthGot.TrySetResult(p.ToString()));
        bystanderConn.On<JsonElement>("notification", p => bystanderGot.TrySetResult(p.ToString()));

        try
        {
            await youthConn.StartAsync();
            await bystanderConn.StartAsync();

            // Mentor messages the youth (allowed: youth.MentorId == mentor).
            var send = await Client.SendAsync(Authorized(HttpMethod.Post, "/api/messages",
                new { toUserId = youth.Id, body = "Great work this week" }, mentor.Token));
            send.EnsureSuccessStatusCode();

            var completed = await Task.WhenAny(youthGot.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.That(completed, Is.EqualTo(youthGot.Task), "expected a real-time notification within 10s");
            Assert.That(youthGot.Task.Result, Does.Contain("New message"));

            // Scoping: the notification was targeted at the recipient's own group, so the bystander,
            // who is connected on the same hub, must not receive it (L2-NFR-005 AC1/AC4).
            await Task.Delay(500);
            Assert.That(bystanderGot.Task.IsCompleted, Is.False, "notification leaked to a non-recipient");
        }
        finally
        {
            await youthConn.DisposeAsync();
            await bystanderConn.DisposeAsync();
        }
    }

    private HttpRequestMessage Authorized(HttpMethod method, string url, object body, string token)
    {
        var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return request;
    }

    private sealed record AuthDto(string AccessToken);
}

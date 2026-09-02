// Acceptance Test
// Traces to: L2-NFR-005 (near-real-time delivery of notifications/messages to the correctly-scoped recipient)
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-005")]
public sealed class RealtimeDeliveryTests : IntegrationTestBase
{
    private async Task<(Guid Id, string Token)> SeedAndSignInAsync(
        string role,
        Guid? mentorId = null,
        Guid? cohortId = null)
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
                CohortId = cohortId,
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
        var youthGotMessage = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        youthConn.On<JsonElement>("notification", p => youthGot.TrySetResult(p.ToString()));
        bystanderConn.On<JsonElement>("notification", p => bystanderGot.TrySetResult(p.ToString()));
        youthConn.On<JsonElement>("message", p => youthGotMessage.TrySetResult(p.ToString()));

        try
        {
            await youthConn.StartAsync();
            await bystanderConn.StartAsync();
            await WaitUntilDeliveryReadyAsync(youth.Id, youthConn);
            await WaitUntilDeliveryReadyAsync(bystander.Id, bystanderConn);

            // Mentor messages the youth (allowed: youth.MentorId == mentor).
            var send = await Client.SendAsync(Authorized(HttpMethod.Post, "/api/messages",
                new { toUserId = youth.Id, body = "Great work this week" }, mentor.Token));
            send.EnsureSuccessStatusCode();

            var completed = await Task.WhenAny(youthGot.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.That(completed, Is.EqualTo(youthGot.Task), "expected a real-time notification within 10s");
            Assert.That(youthGot.Task.Result, Does.Contain("New message"));
            var messageCompleted = await Task.WhenAny(youthGotMessage.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.That(messageCompleted, Is.EqualTo(youthGotMessage.Task), "expected the new thread message in real time");
            Assert.That(youthGotMessage.Task.Result, Does.Contain("Great work this week"));

            // Scoping: the server revalidates the intended recipient, so a bystander connected to the
            // same hub must not receive the notification (L2-NFR-005 AC1/AC4).
            await Task.Delay(500);
            Assert.That(bystanderGot.Task.IsCompleted, Is.False, "notification leaked to a non-recipient");
        }
        finally
        {
            await youthConn.DisposeAsync();
            await bystanderConn.DisposeAsync();
        }
    }

    [Test]
    public async Task A_revoked_session_cannot_receive_passive_user_events()
    {
        var user = await SeedAndSignInAsync(Roles.Youth);
        await using var connection = BuildHubConnection(user.Token);
        var received = Capture(connection, "session-revoked");
        await connection.StartAsync();
        await WaitUntilDeliveryReadyAsync(user.Id, connection);

        using (var scope = Factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ISessionIssuer>()
                .RevokeAllAsync(user.Id);
        }

        await Publisher.PublishToUserAsync(
            user.Id,
            "session-revoked",
            new { state = "revoked" });

        await AssertNotReceivedAsync(received.Task, "a revoked session received a passive user event");
    }

    [Test]
    public async Task An_expired_token_closes_the_socket_and_cannot_receive_passive_events()
    {
        var user = await SeedAndSignInAsync(Roles.Youth);
        var expiresAt = DateTime.UtcNow.AddSeconds(5);
        var shortToken = IssueShortLivedToken(user.Id, Roles.Youth, expiresAt);
        await using var connection = BuildHubConnection(shortToken);
        var received = Capture(connection, "token-expired");
        var closed = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        connection.Closed += _ =>
        {
            closed.TrySetResult();
            return Task.CompletedTask;
        };
        await connection.StartAsync();
        await WaitUntilDeliveryReadyAsync(user.Id, connection);

        var untilExpiry = expiresAt - DateTime.UtcNow + TimeSpan.FromMilliseconds(250);
        if (untilExpiry > TimeSpan.Zero)
        {
            await Task.Delay(untilExpiry);
        }
        await Publisher.PublishToUserAsync(
            user.Id,
            "token-expired",
            new { state = "expired" });

        await AssertNotReceivedAsync(received.Task, "an expired socket received a passive user event");
        Assert.That(
            await Task.WhenAny(closed.Task, Task.Delay(TimeSpan.FromSeconds(5))),
            Is.EqualTo(closed.Task),
            "the hub transport remained open after authentication expiration");
    }

    [Test]
    public async Task A_deactivated_user_cannot_receive_passive_user_events()
    {
        var user = await SeedAndSignInAsync(Roles.Youth);
        await using var connection = BuildHubConnection(user.Token);
        var received = Capture(connection, "account-deactivated");
        await connection.StartAsync();
        await WaitUntilDeliveryReadyAsync(user.Id, connection);

        await UpdateUserAsync(user.Id, current =>
        {
            current.Status = UserStatus.Deactivated;
            current.DeactivatedAt = DateTime.UtcNow;
        });

        await Publisher.PublishToUserAsync(
            user.Id,
            "account-deactivated",
            new { state = "deactivated" });

        await AssertNotReceivedAsync(received.Task, "a deactivated user received a passive user event");
    }

    [Test]
    public async Task A_data_subject_removed_user_cannot_receive_passive_user_events()
    {
        var user = await SeedAndSignInAsync(Roles.Youth);
        await using var connection = BuildHubConnection(user.Token);
        var received = Capture(connection, "data-subject-removed");
        await connection.StartAsync();
        await WaitUntilDeliveryReadyAsync(user.Id, connection);

        await UpdateUserAsync(user.Id, current =>
        {
            current.Status = UserStatus.Removed;
            current.SessionVersion++;
            current.PersonalDataAnonymizedAt = DateTime.UtcNow;
        });

        await Publisher.PublishToUserAsync(
            user.Id,
            "data-subject-removed",
            new { state = "removed" });

        await AssertNotReceivedAsync(received.Task, "a removed data subject received a passive user event");
    }

    [Test]
    public async Task A_role_change_blocks_the_old_role_without_client_activity()
    {
        var user = await SeedAndSignInAsync(Roles.Mentor);
        await using var connection = BuildHubConnection(user.Token);
        var beforeChange = Capture(connection, "mentor-role-current");
        var afterChange = Capture(connection, "mentor-role-stale");
        await connection.StartAsync();
        await WaitUntilDeliveryReadyAsync(user.Id, connection);

        await Publisher.PublishToRoleAsync(
            Roles.Mentor,
            "mentor-role-current",
            new { state = "current" });
        await AssertReceivedAsync(beforeChange.Task, "the current role did not receive its event");

        // Deliberately leave the session version unchanged: delivery independently compares the
        // authenticated role snapshot with the current database role.
        await UpdateUserAsync(user.Id, current => current.Role = Roles.Youth);
        await Publisher.PublishToRoleAsync(
            Roles.Mentor,
            "mentor-role-stale",
            new { state = "stale" });

        await AssertNotReceivedAsync(afterChange.Task, "a stale role received an old-role event");
    }

    [Test]
    public async Task A_cohort_change_blocks_the_old_cohort_and_allows_the_current_cohort()
    {
        var cohorts = await SeedCohortsAsync();
        var user = await SeedAndSignInAsync(Roles.Youth, cohortId: cohorts.First);
        await using var connection = BuildHubConnection(user.Token);
        var beforeChange = Capture(connection, "cohort-before-change");
        var oldCohort = Capture(connection, "old-cohort-after-change");
        var currentCohort = Capture(connection, "current-cohort-after-change");
        await connection.StartAsync();
        await WaitUntilDeliveryReadyAsync(user.Id, connection);

        await Publisher.PublishToCohortAsync(
            cohorts.First,
            "cohort-before-change",
            new { state = "current" });
        await AssertReceivedAsync(beforeChange.Task, "the current cohort did not receive its event");

        await UpdateUserAsync(user.Id, current => current.CohortId = cohorts.Second);
        await Publisher.PublishToCohortAsync(
            cohorts.First,
            "old-cohort-after-change",
            new { state = "stale" });
        await Publisher.PublishToCohortAsync(
            cohorts.Second,
            "current-cohort-after-change",
            new { state = "current" });

        await AssertNotReceivedAsync(oldCohort.Task, "a stale cohort received an old-cohort event");
        await AssertReceivedAsync(currentCohort.Task, "the current cohort did not receive its event");
    }

    private IChangePublisher Publisher => Factory.Services.GetRequiredService<IChangePublisher>();

    private async Task WaitUntilDeliveryReadyAsync(Guid userId, HubConnection connection)
    {
        var eventName = $"realtime-ready-{Guid.NewGuid():N}";
        var received = Capture(connection, eventName);
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!received.Task.IsCompleted && DateTime.UtcNow < deadline)
        {
            await Publisher.PublishToUserAsync(userId, eventName, new { ready = true });
            await Task.WhenAny(received.Task, Task.Delay(100));
        }

        Assert.That(
            received.Task.IsCompleted,
            Is.True,
            "the realtime connection did not become ready for authoritative delivery");
    }

    private static TaskCompletionSource<JsonElement> Capture(
        HubConnection connection,
        string eventName)
    {
        var received = new TaskCompletionSource<JsonElement>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<JsonElement>(eventName, payload => received.TrySetResult(payload));
        return received;
    }

    private static async Task AssertReceivedAsync(Task received, string message)
    {
        var completed = await Task.WhenAny(received, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.That(completed, Is.EqualTo(received), message);
    }

    private static async Task AssertNotReceivedAsync(Task received, string message)
    {
        var completed = await Task.WhenAny(received, Task.Delay(750));
        Assert.That(completed, Is.Not.EqualTo(received), message);
    }

    private async Task UpdateUserAsync(Guid userId, Action<User> update)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("Realtime test user was not found.");
        update(user);
        await db.SaveChangesAsync();
    }

    private async Task<(Guid First, Guid Second)> SeedCohortsAsync()
    {
        var now = DateTime.UtcNow;
        var first = new Cohort
        {
            Name = $"Realtime cohort {Guid.NewGuid():N}",
            TermStart = now.AddMonths(-1),
            TermEnd = now.AddMonths(2),
        };
        var second = new Cohort
        {
            Name = $"Realtime cohort {Guid.NewGuid():N}",
            TermStart = now.AddMonths(-1),
            TermEnd = now.AddMonths(2),
        };
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Cohorts.AddRange(first, second);
        await db.SaveChangesAsync();
        return (first.Id, second.Id);
    }

    private static string IssueShortLivedToken(Guid userId, string role, DateTime expiresAt)
    {
        const string signingKey = "waypoint-integration-tests-signing-key-32chars-min!!";
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "Waypoint",
            audience: "Waypoint",
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("session_version", "0"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ],
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: expiresAt,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private HttpRequestMessage Authorized(HttpMethod method, string url, object body, string token)
    {
        var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return request;
    }

    private sealed record AuthDto(string AccessToken);
}

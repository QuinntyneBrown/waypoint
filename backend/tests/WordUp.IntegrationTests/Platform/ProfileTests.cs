// Acceptance Test
// Traces to: L2-PLATFORM-002
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-002")]
public sealed class ProfileTests : IntegrationTestBase
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

    private async Task<(Guid Id, string Email)> SeedUserAsync(string role)
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
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task<string> SignInAsync(string email, string password = "Passw0rd!")
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-PLATFORM-002")]
    public async Task A_user_updates_their_display_name_and_email()
    {
        // Traces to: L2-PLATFORM-002
        var user = await SeedUserAsync(Roles.Youth);
        var token = await SignInAsync(user.Email);

        var newEmail = $"updated.{Guid.NewGuid():N}@example.com";
        var response = await SendAsync(HttpMethod.Put, "/api/me/profile", body: new
        {
            displayName = "Updated Name",
            email = newEmail,
            pronouns = "they/them",
            photoUrl = "https://example.com/me.png",
        }, token: token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = db.Users.Single(u => u.Id == user.Id);
        Assert.That(stored.DisplayName, Is.EqualTo("Updated Name"));
        Assert.That(stored.Email, Is.EqualTo(newEmail));
        Assert.That(stored.Pronouns, Is.EqualTo("they/them"));
    }

    [Test]
    [Property("Req", "L2-PLATFORM-002")]
    public async Task Changing_the_password_requires_the_correct_current_password_and_then_the_new_password_signs_in()
    {
        // Traces to: L2-PLATFORM-002
        var user = await SeedUserAsync(Roles.Youth);
        var token = await SignInAsync(user.Email);

        // A wrong current password is rejected.
        var wrong = await SendAsync(HttpMethod.Post, "/api/me/change-password", body: new
        {
            currentPassword = "Wr0ngPass!",
            newPassword = "NewPassw0rd!",
            confirmPassword = "NewPassw0rd!",
        }, token: token);
        Assert.That(wrong.IsSuccessStatusCode, Is.False);

        // The correct current password succeeds.
        var changed = await SendAsync(HttpMethod.Post, "/api/me/change-password", body: new
        {
            currentPassword = "Passw0rd!",
            newPassword = "NewPassw0rd!",
            confirmPassword = "NewPassw0rd!",
        }, token: token);
        Assert.That(changed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Signing in with the NEW password works.
        var newToken = await SignInAsync(user.Email, "NewPassw0rd!");
        Assert.That(newToken, Is.Not.Empty);
    }
}

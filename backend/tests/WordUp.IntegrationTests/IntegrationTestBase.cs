using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests;

/// <summary>
/// Base fixture: stands up a fresh LocalDB database + the API host once per test class,
/// and drops the database afterwards. NUnit's async one-time hooks stand in for xUnit's IAsyncLifetime.
/// </summary>
public abstract class IntegrationTestBase
{
    protected SqlServerWebAppFactory Factory = null!;
    protected HttpClient Client = null!;

    [OneTimeSetUp]
    public async Task GlobalSetupAsync()
    {
        Factory = new SqlServerWebAppFactory();

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        Client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task GlobalTeardownAsync()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
        }

        Client.Dispose();
        await Factory.DisposeAsync();
    }

    /// <summary>Seeds an active user with the given role and returns a signed-in access token.</summary>
    protected async Task<string> CreateUserAndGetTokenAsync(string role, string password = "Passw0rd!")
    {
        var email = $"{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            db.Users.Add(new User
            {
                Email = email,
                DisplayName = role,
                Role = role,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(password),
            });
            await db.SaveChangesAsync();
        }

        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    protected static void Authorize(HttpRequestMessage request, string accessToken) =>
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
}

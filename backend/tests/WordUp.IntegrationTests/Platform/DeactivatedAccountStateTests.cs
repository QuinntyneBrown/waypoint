// Acceptance Test
// Traces to: L2-PLATFORM-010
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-010")]
public sealed class DeactivatedAccountStateTests : IntegrationTestBase
{
    [Test]
    [Property("Req", "L2-PLATFORM-010")]
    public async Task Signing_in_to_a_deactivated_account_with_correct_credentials_is_refused()
    {
        // Traces to: L2-PLATFORM-010
        var email = $"deactivated.{Guid.NewGuid():N}@example.com";
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            db.Users.Add(new User
            {
                Email = email,
                DisplayName = "Deactivated Youth",
                Role = Roles.Youth,
                Status = UserStatus.Deactivated,
                PasswordHash = hasher.Hash("Passw0rd!"),
            });
            await db.SaveChangesAsync();
        }

        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("inactive"));
    }
}

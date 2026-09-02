// Test-support endpoint coverage — untraced by design: this endpoint exists only to let the
// HTTP-only E2E acceptance harness provision non-Youth identities (which otherwise require
// in-process DbContext seeding). It is mapped only in the Testing environment.
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Platform;

[TestFixture]
public sealed class TestSupportSeedTests : IntegrationTestBase
{
    [Test]
    public async Task Seed_user_endpoint_creates_an_active_role_user_that_can_sign_in()
    {
        var email = $"seed.{Guid.NewGuid():N}@example.com";

        var create = await Client.PostAsJsonAsync("/api/_test/users", new
        {
            email,
            password = "Passw0rd!",
            displayName = "Seeded Admin",
            role = Roles.ProgramAdministrator,
        });
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // The row exists with the requested role and is active.
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var row = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            Assert.That(row, Is.Not.Null);
            Assert.That(row!.Role, Is.EqualTo(Roles.ProgramAdministrator));
            Assert.That(row.Status, Is.EqualTo(UserStatus.Active));
        }

        // And the seeded identity authenticates through the real sign-in endpoint.
        var signIn = await Client.PostAsJsonAsync(
            "/api/auth/sign-in", new { email, password = "Passw0rd!" });
        Assert.That(signIn.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Seed_user_endpoint_rejects_an_unknown_role()
    {
        var create = await Client.PostAsJsonAsync("/api/_test/users", new
        {
            email = $"seed.{Guid.NewGuid():N}@example.com",
            password = "Passw0rd!",
            displayName = "Nobody",
            role = "SuperUser",
        });
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}

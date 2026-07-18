// Acceptance Test
// Traces to: L2-PLATFORM-011
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-011")]
public sealed class AccessGateTests : IntegrationTestBase
{
    private async Task<(Guid Id, string Email)> SeedYouthAsync(Guid? cohortId)
    {
        var email = $"youth.{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = email,
            DisplayName = "Gate Youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CohortId = cohortId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task SeedConsentAsync(Guid youthUserId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.GuardianConsents.Add(new GuardianConsent
        {
            YouthUserId = youthUserId,
            GuardianName = "Parent Guardian",
            Relationship = "parent",
            ParticipationConsent = true,
            Signature = "Parent Guardian",
        });
        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedCohortAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cohort = new Cohort
        {
            Name = "Gate Cohort",
            TermStart = DateTime.UtcNow.AddDays(-7),
            TermEnd = DateTime.UtcNow.AddDays(60),
        };
        db.Cohorts.Add(cohort);
        await db.SaveChangesAsync();
        return cohort.Id;
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    private async Task<AccessStateDto> GetAccessAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/me/access");
        Authorize(request, token);
        var response = await Client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return (await response.Content.ReadFromJsonAsync<AccessStateDto>())!;
    }

    [Test]
    [Property("Req", "L2-PLATFORM-011")]
    public async Task A_youth_without_a_cohort_or_consent_is_not_permitted_to_participate()
    {
        // Traces to: L2-PLATFORM-011
        var youth = await SeedYouthAsync(cohortId: null);
        var token = await SignInAsync(youth.Email);

        var access = await GetAccessAsync(token);

        Assert.That(access.HasCohort, Is.False);
        Assert.That(access.ConsentRequired, Is.True);
        Assert.That(access.ParticipationAllowed, Is.False);
    }

    [Test]
    [Property("Req", "L2-PLATFORM-011")]
    public async Task A_youth_placed_in_a_cohort_with_consent_on_file_is_permitted_to_participate()
    {
        // Traces to: L2-PLATFORM-011
        var cohortId = await SeedCohortAsync();
        var youth = await SeedYouthAsync(cohortId: cohortId);
        await SeedConsentAsync(youth.Id);
        var token = await SignInAsync(youth.Email);

        var access = await GetAccessAsync(token);

        Assert.That(access.HasCohort, Is.True);
        Assert.That(access.ConsentRequired, Is.False);
        Assert.That(access.ParticipationAllowed, Is.True);
    }

    private sealed record AccessStateDto(bool HasCohort, bool ConsentRequired, bool ParticipationAllowed);
}

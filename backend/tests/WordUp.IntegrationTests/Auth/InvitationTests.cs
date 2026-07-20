using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Auth;

// Acceptance Test
// Traces to: L2-AUTH-003 (invitation issuance), L2-AUTH-004 (accept/resend/revoke),
//            L2-AUTH-008 (password policy on accept), L2-AUTH-009 (authorization on invite)
[TestFixture]
[Property("Req", "L2-AUTH-004")]
public sealed class InvitationTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string? accessToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        if (accessToken is not null)
        {
            Authorize(request, accessToken);
        }
        return await Client.SendAsync(request);
    }

    private string TokenFor(string email)
    {
        var sent = Factory.Emails.LastTo(email);
        Assert.That(sent, Is.Not.Null, "an invitation email should have been sent");
        return Regex.Match(sent!.Body, @"token=(\S+)").Groups[1].Value;
    }

    [Test]
    [Property("Req", "L2-AUTH-003")]
    [Property("Req", "L2-AUTH-004")]
    public async Task Admin_invites_a_mentor_who_accepts_and_is_activated_with_that_role()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var invitee = $"newmentor.{Guid.NewGuid():N}@example.com";

        var invite = await PostAsync("/api/invitations", new { email = invitee, role = Roles.Mentor }, adminToken);
        Assert.That(invite.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var accept = await PostAsync("/api/invitations/accept",
            new { token = TokenFor(invitee), displayName = "Grace Adeyemi", password = "Passw0rd!", agreeToGuidelines = true });
        Assert.That(accept.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var session = await accept.Content.ReadFromJsonAsync<AuthResult>();
        Assert.That(session!.Role, Is.EqualTo(Roles.Mentor));
        Assert.That(session.Email, Is.EqualTo(invitee));
        Assert.That(session.AccessToken, Is.Not.Empty);
    }

    [Test]
    [Property("Req", "L2-AUTH-009")]
    public async Task A_youth_cannot_issue_invitations()
    {
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var invite = await PostAsync("/api/invitations",
            new { email = $"x.{Guid.NewGuid():N}@example.com", role = Roles.Youth }, youthToken);

        Assert.That(invite.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-AUTH-004")]
    public async Task A_revoked_invitation_cannot_be_accepted()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var invitee = $"revoked.{Guid.NewGuid():N}@example.com";

        var invite = await PostAsync("/api/invitations", new { email = invitee, role = Roles.Youth }, adminToken);
        var invitationId = (await invite.Content.ReadFromJsonAsync<InviteResultDto>())!.InvitationId;
        var token = TokenFor(invitee);

        var revoke = await PostAsync($"/api/invitations/{invitationId}/revoke", new { }, adminToken);
        Assert.That(revoke.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var accept = await PostAsync("/api/invitations/accept",
            new { token, displayName = "Nope", password = "Passw0rd!", agreeToGuidelines = true });
        Assert.That((int)accept.StatusCode, Is.EqualTo(410));
    }

    [Test]
    [Property("Req", "L2-AUTH-008")]
    public async Task Accepting_with_a_password_that_violates_policy_is_rejected()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var invitee = $"weakpw.{Guid.NewGuid():N}@example.com";
        await PostAsync("/api/invitations", new { email = invitee, role = Roles.Youth }, adminToken);

        var accept = await PostAsync("/api/invitations/accept",
            new { token = TokenFor(invitee), displayName = "Weak", password = "short", agreeToGuidelines = true });

        Assert.That(accept.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Property("Req", "L2-AUTH-003")]
    public async Task An_invitation_cannot_be_issued_for_an_existing_account()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var existingEmail = $"owned.{Guid.NewGuid():N}@example.com";

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            db.Users.Add(new User
            {
                Email = existingEmail,
                DisplayName = "Existing account",
                PasswordHash = hasher.Hash("Original1!"),
                Role = Roles.Guardian,
                Status = UserStatus.Active,
            });
            await db.SaveChangesAsync();
        }

        var response = await PostAsync(
            "/api/invitations",
            new { email = existingEmail.ToUpperInvariant(), role = Roles.Youth },
            adminToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(Factory.Emails.AllTo(existingEmail), Is.Empty);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(await db.Invitations.CountAsync(item => item.Email == existingEmail), Is.Zero);
        }
    }

    [Test]
    [Property("Req", "L2-AUTH-003")]
    public async Task A_bulk_invitation_is_all_or_nothing_when_an_address_has_an_account()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var existingEmail = $"bulk-owned.{Guid.NewGuid():N}@example.com";
        var availableEmail = $"bulk-new.{Guid.NewGuid():N}@example.com";

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User
            {
                Email = existingEmail,
                DisplayName = "Existing account",
                PasswordHash = "not-used",
                Role = Roles.Youth,
                Status = UserStatus.Active,
            });
            await db.SaveChangesAsync();
        }

        var response = await PostAsync(
            "/api/invitations/bulk",
            new { emails = new[] { availableEmail, existingEmail }, role = Roles.Youth },
            adminToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(Factory.Emails.AllTo(availableEmail), Is.Empty);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(
                await db.Invitations.CountAsync(item => item.Email == availableEmail || item.Email == existingEmail),
                Is.Zero);
        }
    }

    [Test]
    [Property("Req", "L2-AUTH-004")]
    public async Task Acceptance_cannot_mutate_an_account_created_after_invitation_issuance()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var invitee = $"late-account.{Guid.NewGuid():N}@example.com";
        var invite = await PostAsync(
            "/api/invitations",
            new { email = invitee, role = Roles.Youth },
            adminToken);
        var invitationId = (await invite.Content.ReadFromJsonAsync<InviteResultDto>())!.InvitationId;
        var invitationToken = TokenFor(invitee);
        Guid existingUserId;
        string originalPasswordHash;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            originalPasswordHash = hasher.Hash("Original1!");
            var existing = new User
            {
                Email = invitee,
                DisplayName = "Do not change",
                PasswordHash = originalPasswordHash,
                Role = Roles.Guardian,
                Status = UserStatus.Active,
                OnboardingCompleted = true,
            };
            existingUserId = existing.Id;
            db.Users.Add(existing);
            await db.SaveChangesAsync();
        }

        var resend = await PostAsync($"/api/invitations/{invitationId}/resend", new { }, adminToken);
        var accept = await PostAsync(
            "/api/invitations/accept",
            new
            {
                token = invitationToken,
                displayName = "Attacker-controlled name",
                password = "Changed1!",
                agreeToGuidelines = true,
            });

        Assert.Multiple(() =>
        {
            Assert.That(resend.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That((int)accept.StatusCode, Is.EqualTo(410));
        });
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var unchanged = await db.Users.SingleAsync(item => item.Id == existingUserId);
            Assert.Multiple(() =>
            {
                Assert.That(unchanged.DisplayName, Is.EqualTo("Do not change"));
                Assert.That(unchanged.PasswordHash, Is.EqualTo(originalPasswordHash));
                Assert.That(unchanged.Role, Is.EqualTo(Roles.Guardian));
                Assert.That(unchanged.OnboardingCompleted, Is.True);
                Assert.That(db.RefreshTokens.Count(item => item.UserId == existingUserId), Is.Zero);
            });
        }
    }

    [Test]
    [Property("Req", "L2-AUTH-004")]
    public async Task Simultaneous_acceptance_issues_exactly_one_account_and_session()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var invitee = $"single-use.{Guid.NewGuid():N}@example.com";
        await PostAsync("/api/invitations", new { email = invitee, role = Roles.Mentor }, adminToken);
        var token = TokenFor(invitee);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<HttpResponseMessage> AcceptAsync(string displayName)
        {
            await start.Task;
            return await PostAsync(
                "/api/invitations/accept",
                new { token, displayName, password = "Passw0rd!", agreeToGuidelines = true });
        }

        var firstTask = AcceptAsync("First claimant");
        var secondTask = AcceptAsync("Second claimant");
        start.SetResult();
        var responses = await Task.WhenAll(firstTask, secondTask);

        Assert.Multiple(() =>
        {
            Assert.That(responses.Count(response => response.StatusCode == HttpStatusCode.OK), Is.EqualTo(1));
            Assert.That(responses.Count(response => (int)response.StatusCode == 410), Is.EqualTo(1));
        });
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var users = await db.Users.Where(item => item.Email == invitee).ToListAsync();
            Assert.That(users, Has.Count.EqualTo(1));
            Assert.That(await db.RefreshTokens.CountAsync(item => item.UserId == users[0].Id), Is.EqualTo(1));
            var invitation = await db.Invitations.SingleAsync(item => item.Email == invitee);
            Assert.That(invitation.Status, Is.EqualTo(InvitationStatus.Accepted));
        }
    }

    private sealed record InviteResultDto(Guid InvitationId);
}

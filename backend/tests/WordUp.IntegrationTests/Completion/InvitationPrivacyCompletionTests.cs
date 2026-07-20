using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Authentication.Invitations;
using WordUp.Application.PlatformServices.Privacy;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Completion;

// Traces to: L2-AUTH-003, L2-AUTH-004, L2-NFR-003
[TestFixture]
[Property("Req", "L2-AUTH-004")]
[Property("Req", "L2-NFR-003")]
public sealed class InvitationPrivacyCompletionTests : IntegrationTestBase
{
    [Test]
    public async Task Invitation_list_context_expiry_and_replacement_are_complete_and_safe()
    {
        var (admin, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var invitee = $"completion.invite.{Guid.NewGuid():N}@example.com";
        var create = await SendAsync(
            HttpMethod.Post,
            "/api/invitations",
            new { email = invitee, role = Roles.Youth },
            adminToken);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var invitationId = (await create.Content.ReadFromJsonAsync<InviteResultDto>())!.InvitationId;
        var inviteEmail = Factory.Emails.LastTo(invitee);
        var rawToken = Regex.Match(inviteEmail!.Body, @"token=([A-Za-z0-9_-]+)").Groups[1].Value;
        Assert.That(inviteEmail.Body, Does.Contain("http://localhost:4200/auth/invite?token="));

        var context = await Client.GetFromJsonAsync<InvitationContext>(
            $"/api/invitations/context?token={Uri.EscapeDataString(rawToken)}");
        Assert.Multiple(() =>
        {
            Assert.That(context!.State, Is.EqualTo("valid"));
            Assert.That(context.MaskedEmail, Does.Not.Contain(invitee.Split('@')[0]));
            Assert.That(context.Role, Is.EqualTo(Roles.Youth));
        });

        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/invitations?status=Pending");
        Authorize(listRequest, adminToken);
        var listResponse = await Client.SendAsync(listRequest);
        var pending = await listResponse.Content.ReadFromJsonAsync<InvitationPage>();
        Assert.That(pending!.Items.Any(item => item.Id == invitationId), Is.True);

        await WithDbAsync(async db =>
        {
            var invitation = await db.Invitations.SingleAsync(item => item.Id == invitationId);
            invitation.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        });
        var expiredRequest = new HttpRequestMessage(HttpMethod.Get, "/api/invitations?status=Expired");
        Authorize(expiredRequest, adminToken);
        var expiredResponse = await Client.SendAsync(expiredRequest);
        var expired = await expiredResponse.Content.ReadFromJsonAsync<InvitationPage>();
        Assert.That(expired!.Items.Single(item => item.Id == invitationId).Status, Is.EqualTo("Expired"));

        var before = Factory.Emails.AllTo(admin.Email).Count;
        var replacement = await Client.PostAsJsonAsync(
            "/api/invitations/replacement-request",
            new { token = rawToken });
        Assert.Multiple(() =>
        {
            Assert.That(replacement.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(Factory.Emails.AllTo(admin.Email).Count, Is.EqualTo(before + 1));
            Assert.That(Factory.Emails.LastTo(admin.Email)!.Body, Does.Not.Contain(rawToken));
        });
    }

    [Test]
    public async Task Bulk_invitation_scope_is_persisted_and_invalid_addresses_are_rejected()
    {
        var (admin, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var mentor = await SeedUserAsync(Roles.Mentor);
        var cohort = new Cohort { Name = $"Completion cohort {Guid.NewGuid():N}" };
        await WithDbAsync(async db =>
        {
            db.Cohorts.Add(cohort);
            await db.SaveChangesAsync();
        });

        var invalid = await SendAsync(
            HttpMethod.Post,
            "/api/invitations/bulk",
            new { emails = new[] { "not-an-email" }, role = Roles.Youth, cohortId = cohort.Id, mentorId = mentor.Id },
            adminToken);
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var email = $"bulk.scope.{Guid.NewGuid():N}@example.com";
        var valid = await SendAsync(
            HttpMethod.Post,
            "/api/invitations/bulk",
            new { emails = new[] { email }, role = Roles.Youth, cohortId = cohort.Id, mentorId = mentor.Id },
            adminToken);
        Assert.That(valid.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await WithDbAsync(async db =>
        {
            var invitation = await db.Invitations.SingleAsync(item => item.Email == email);
            Assert.Multiple(() =>
            {
                Assert.That(invitation.CohortId, Is.EqualTo(cohort.Id));
                Assert.That(invitation.MentorId, Is.EqualTo(mentor.Id));
            });
        });
    }

    [Test]
    public async Task Typed_privacy_correction_appears_in_self_history_and_notifies_on_completion()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var correctedEmail = $"privacy.corrected.{Guid.NewGuid():N}@example.com";
        var create = await SendAsync(
            HttpMethod.Post,
            "/api/me/data-requests",
            new
            {
                actionType = "correction",
                details = "Please apply these profile corrections.",
                correction = new
                {
                    displayName = "Corrected participant",
                    email = correctedEmail,
                    pronouns = "they/them",
                },
            },
            subjectToken);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var created = await create.Content.ReadFromJsonAsync<DataSubjectRequestResult>();

        var historyRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me/data-requests");
        Authorize(historyRequest, subjectToken);
        var historyResponse = await Client.SendAsync(historyRequest);
        var history = await historyResponse.Content.ReadFromJsonAsync<MyDataSubjectRequestPage>();
        Assert.Multiple(() =>
        {
            Assert.That(history!.Total, Is.EqualTo(1));
            Assert.That(history.Items[0].Correction!.Email, Is.EqualTo(correctedEmail));
        });

        var fulfill = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{created!.Id}/fulfill",
            new { resolutionNote = "Verified and applied." },
            adminToken);
        Assert.That(fulfill.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await WithDbAsync(async db =>
        {
            var changed = await db.Users.SingleAsync(item => item.Id == subject.Id);
            Assert.Multiple(() =>
            {
                Assert.That(changed.DisplayName, Is.EqualTo("Corrected participant"));
                Assert.That(changed.Email, Is.EqualTo(correctedEmail));
                Assert.That(changed.Pronouns, Is.EqualTo("they/them"));
                Assert.That(Factory.Emails.LastTo(correctedEmail), Is.Not.Null);
            });
        });
    }

    private async Task<(User User, string Token)> SeedAndSignInAsync(string role)
    {
        var user = await SeedUserAsync(role);
        var response = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { email = user.Email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (user, auth!.AccessToken);
    }

    private async Task<User> SeedUserAsync(string role)
    {
        User? user = null;
        await WithDbAsync(async db =>
        {
            var hasher = Factory.Services.GetRequiredService<IPasswordHasher>();
            user = new User
            {
                Email = $"completion.{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com",
                DisplayName = $"Completion {role}",
                Role = role,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        });
        return user!;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object body,
        string token)
    {
        var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task WithDbAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    private sealed record InviteResultDto(Guid InvitationId);
}

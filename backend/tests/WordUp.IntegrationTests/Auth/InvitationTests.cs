using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using WordUp.Application.Authentication.Auth;

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

    private sealed record InviteResultDto(Guid InvitationId);
}

using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Authentication.Invitations;

/// <summary>Composes the invitation email (shared by issue + resend). The raw token is delivered here only.</summary>
public static class InvitationEmail
{
    public static Task SendAsync(
        IEmailSender email,
        IApplicationLinkBuilder links,
        string toEmail,
        string rawToken,
        CancellationToken cancellationToken) =>
        email.SendAsync(
            toEmail,
            "Your Word Up invitation",
            $"You've been invited to join Word Up. Set up your account: {links.Portal($"/auth/invite?token={Uri.EscapeDataString(rawToken)}")}",
            cancellationToken);
}

using WordUp.Application.Abstractions;

namespace WordUp.Application.Invitations;

/// <summary>Composes the invitation email (shared by issue + resend). The raw token is delivered here only.</summary>
public static class InvitationEmail
{
    public static Task SendAsync(IEmailSender email, string toEmail, string rawToken, CancellationToken cancellationToken) =>
        email.SendAsync(
            toEmail,
            "Your Word Up invitation",
            $"You've been invited to join Word Up. Set up your account: token={rawToken}",
            cancellationToken);
}

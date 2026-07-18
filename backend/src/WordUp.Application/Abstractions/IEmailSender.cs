namespace WordUp.Application.Abstractions;

/// <summary>Sends transactional email (invitations, password reset, consent copies).</summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}

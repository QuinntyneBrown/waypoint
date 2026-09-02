namespace Waypoint.Application.Shared.Abstractions;

/// <summary>Sends transactional email (invitations, password reset, consent copies).</summary>
public interface IEmailSender
{
    Task SendAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default,
        string? idempotencyKey = null,
        Guid? subjectUserId = null,
        bool purgeAfterDelivery = false,
        Guid? recipientUserId = null,
        bool isPrivacyRemovalConfirmation = false);
}

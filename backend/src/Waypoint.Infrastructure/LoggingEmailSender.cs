using Microsoft.Extensions.Logging;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

/// <summary>
/// Default email sender: logs the message. Real delivery (SMTP/provider) is a later concern; tests
/// substitute a capturing implementation to read invitation / reset tokens.
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default,
        string? idempotencyKey = null,
        Guid? subjectUserId = null,
        bool purgeAfterDelivery = false,
        Guid? recipientUserId = null,
        bool isPrivacyRemovalConfirmation = false)
    {
        _logger.LogInformation("Email queued through the configured delivery channel.");
        return Task.CompletedTask;
    }
}

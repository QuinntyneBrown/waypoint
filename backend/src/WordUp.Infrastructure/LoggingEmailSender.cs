using Microsoft.Extensions.Logging;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

/// <summary>
/// Default email sender: logs the message. Real delivery (SMTP/provider) is a later concern; tests
/// substitute a capturing implementation to read invitation / reset tokens.
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Email queued to {Recipient}: {Subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}

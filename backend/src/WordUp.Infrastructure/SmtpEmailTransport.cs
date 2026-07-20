using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace WordUp.Infrastructure;

public interface ISmtpEmailTransport
{
    Task SendAsync(OutboundEmail message, CancellationToken cancellationToken);
}

public sealed class SmtpEmailTransport : ISmtpEmailTransport
{
    private readonly SmtpOptions _options;

    public SmtpEmailTransport(IOptions<SmtpOptions> options) => _options = options.Value;

    public async Task SendAsync(OutboundEmail envelope, CancellationToken cancellationToken)
    {
        if (!_options.Enabled) throw new InvalidOperationException("SMTP delivery is disabled.");

        var message = new MimeMessage
        {
            MessageId = $"<{envelope.Id:N}@wordup.local>",
            Subject = envelope.Subject,
            Body = new TextPart("plain") { Text = envelope.Body },
        };
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(envelope.ToEmail));

        using var client = new SmtpClient { Timeout = checked(_options.TimeoutSeconds * 1000) };
        var socketOptions = _options.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : _options.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, socketOptions, timeout.Token);
            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(
                    _options.Username,
                    _options.Password ?? string.Empty,
                    timeout.Token);
            }
            // A successful SendAsync is the delivery boundary. Cleanup failures after this point
            // must never cause an accepted Message-Id to be retried.
            await client.SendAsync(message, timeout.Token);
        }
        finally
        {
            if (client.IsConnected)
            {
                using var disconnectTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    await client.DisconnectAsync(true, disconnectTimeout.Token);
                }
                catch
                {
                    // Best effort only: the SMTP server may already have accepted the message.
                }
            }
        }
    }
}

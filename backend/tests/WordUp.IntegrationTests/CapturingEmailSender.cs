using System.Collections.Concurrent;
using WordUp.Application.Abstractions;

namespace WordUp.IntegrationTests;

/// <summary>Test double for <see cref="IEmailSender"/> that records sent messages so specs can read
/// invitation / reset tokens (which are only delivered by email).</summary>
public sealed class CapturingEmailSender : IEmailSender
{
    public sealed record SentEmail(string To, string Subject, string Body);

    private readonly ConcurrentQueue<SentEmail> _sent = new();

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _sent.Enqueue(new SentEmail(toEmail, subject, body));
        return Task.CompletedTask;
    }

    public SentEmail? LastTo(string email) =>
        _sent.ToArray().LastOrDefault(e => string.Equals(e.To, email, StringComparison.OrdinalIgnoreCase));
}

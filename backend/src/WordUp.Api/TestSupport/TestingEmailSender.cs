using System.Collections.Concurrent;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Api.TestSupport;

/// <summary>
/// Testing-environment-only transactional email sink. Production never registers or exposes it.
/// </summary>
public sealed class TestingEmailSender : IEmailSender
{
    public sealed record SentEmail(string To, string Subject, string Body);

    private readonly ConcurrentQueue<SentEmail> _sent = new();

    public Task SendAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        _sent.Enqueue(new SentEmail(toEmail, subject, body));
        return Task.CompletedTask;
    }

    public SentEmail? LastTo(string email) =>
        _sent.LastOrDefault(item => item.To.Equals(email, StringComparison.OrdinalIgnoreCase));
}

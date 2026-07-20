using System.ComponentModel.DataAnnotations;

namespace WordUp.Infrastructure;

public sealed class SmtpOptions
{
    public bool Enabled { get; set; }

    [MaxLength(255)]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; }
    public bool UseStartTls { get; set; } = true;

    [MaxLength(256)]
    public string? Username { get; set; }

    [MaxLength(1024)]
    public string? Password { get; set; }

    [EmailAddress, MaxLength(256)]
    public string FromEmail { get; set; } = string.Empty;

    [MaxLength(128)]
    public string FromName { get; set; } = "Word Up";

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Delivery failures stop being retried after this many attempts.</summary>
    [Range(1, 100)]
    public int MaxAttempts { get; set; } = 10;

    /// <summary>Sent and permanently failed envelopes are retained this many days for operations.</summary>
    [Range(1, 3650)]
    public int OutboxRetentionDays { get; set; } = 30;
}

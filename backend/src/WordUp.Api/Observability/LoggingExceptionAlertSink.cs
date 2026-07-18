namespace WordUp.Api.Observability;

/// <summary>
/// Emits an alert-grade log record. Production logging providers route Critical records to the
/// configured operations alert destination without coupling application code to one vendor.
/// </summary>
public sealed class LoggingExceptionAlertSink : IExceptionAlertSink
{
    private readonly ILogger<LoggingExceptionAlertSink> _logger;

    public LoggingExceptionAlertSink(ILogger<LoggingExceptionAlertSink> logger) => _logger = logger;

    public Task NotifyAsync(Exception exception, string correlationId, string method, string path)
    {
        _logger.LogCritical(
            exception,
            "Unhandled API exception alert. CorrelationId={CorrelationId} Method={Method} Path={Path}",
            correlationId,
            method,
            path);
        return Task.CompletedTask;
    }
}

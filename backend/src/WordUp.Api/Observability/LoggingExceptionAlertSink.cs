namespace WordUp.Api.Observability;

/// <summary>
/// Emits an alert-grade log record. Production logging providers route Critical records to the
/// configured operations alert destination without coupling application code to one vendor.
/// </summary>
public sealed class LoggingExceptionAlertSink : IExceptionAlertSink
{
    private readonly ILogger<LoggingExceptionAlertSink> _logger;

    public LoggingExceptionAlertSink(ILogger<LoggingExceptionAlertSink> logger) => _logger = logger;

    public Task NotifyAsync(string exceptionType, string correlationId, string method, string route)
    {
        _logger.LogCritical(
            "Unhandled API exception alert. CorrelationId={CorrelationId} ExceptionType={ExceptionType} " +
            "Method={Method} Route={Route}",
            correlationId,
            exceptionType,
            method,
            route);
        return Task.CompletedTask;
    }
}

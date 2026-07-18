using WordUp.Api.Observability;

namespace WordUp.IntegrationTests;

public sealed record CapturedExceptionAlert(
    Exception Exception,
    string CorrelationId,
    string Method,
    string Path);

public sealed class CapturingExceptionAlertSink : IExceptionAlertSink
{
    private readonly List<CapturedExceptionAlert> _alerts = new();
    public IReadOnlyList<CapturedExceptionAlert> Alerts => _alerts;

    public Task NotifyAsync(Exception exception, string correlationId, string method, string path)
    {
        _alerts.Add(new CapturedExceptionAlert(exception, correlationId, method, path));
        return Task.CompletedTask;
    }
}

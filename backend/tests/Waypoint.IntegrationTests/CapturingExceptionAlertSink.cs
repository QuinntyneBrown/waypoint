using Waypoint.Api.Observability;

namespace Waypoint.IntegrationTests;

public sealed record CapturedExceptionAlert(
    string ExceptionType,
    string CorrelationId,
    string Method,
    string Route);

public sealed class CapturingExceptionAlertSink : IExceptionAlertSink
{
    private readonly List<CapturedExceptionAlert> _alerts = new();
    public IReadOnlyList<CapturedExceptionAlert> Alerts => _alerts;

    public Task NotifyAsync(string exceptionType, string correlationId, string method, string route)
    {
        _alerts.Add(new CapturedExceptionAlert(exceptionType, correlationId, method, route));
        return Task.CompletedTask;
    }
}

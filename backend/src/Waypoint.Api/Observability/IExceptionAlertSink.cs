namespace Waypoint.Api.Observability;

public interface IExceptionAlertSink
{
    Task NotifyAsync(string exceptionType, string correlationId, string method, string route);
}

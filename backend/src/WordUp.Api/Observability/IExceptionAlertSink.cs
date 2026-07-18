namespace WordUp.Api.Observability;

public interface IExceptionAlertSink
{
    Task NotifyAsync(Exception exception, string correlationId, string method, string path);
}

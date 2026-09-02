using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Waypoint.Infrastructure;

public sealed class ScheduledNotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ScheduledNotificationWorker> _logger;

    public ScheduledNotificationWorker(
        IServiceScopeFactory scopes,
        IHostEnvironment environment,
        ILogger<ScheduledNotificationWorker> logger)
    {
        _scopes = scopes;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_environment.IsEnvironment("Testing")) return;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<ScheduledNotificationProcessor>()
                    .RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "Scheduled notification worker failed with {ExceptionType}.",
                    exception.GetType().Name);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Waypoint.Infrastructure;

public sealed class OutboundEmailWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<OutboundEmailWorker> _logger;

    public OutboundEmailWorker(
        IServiceScopeFactory scopes,
        IHostEnvironment environment,
        ILogger<OutboundEmailWorker> logger)
    {
        _scopes = scopes;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_environment.IsEnvironment("Testing")) return;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        var nextCleanupAt = DateTime.MinValue;
        do
        {
            var now = DateTime.UtcNow;
            if (now >= nextCleanupAt)
            {
                try
                {
                    using var scope = _scopes.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IOutboundEmailProcessorRunner>();
                    await processor.PurgeExpiredAsync(stoppingToken);
                    nextCleanupAt = now.AddHours(1);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    nextCleanupAt = now.AddMinutes(5);
                    _logger.LogError(
                        "Outbound email retention cleanup failed with {ExceptionType}.",
                        exception.GetType().Name);
                }
            }

            try
            {
                using var scope = _scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IOutboundEmailProcessorRunner>()
                    .RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // Exception messages can contain SMTP/recipient detail. Log type only.
                _logger.LogError(
                    "Outbound email worker failed with {ExceptionType}.",
                    exception.GetType().Name);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

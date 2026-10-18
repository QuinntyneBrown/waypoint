using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Waypoint.Infrastructure;

/// <summary>Drains queued lesson videos in the background. Idle in the Testing environment.</summary>
public sealed class VideoTranscodeWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<VideoTranscodeWorker> _logger;

    public VideoTranscodeWorker(
        IServiceScopeFactory scopes,
        IHostEnvironment environment,
        ILogger<VideoTranscodeWorker> logger)
    {
        _scopes = scopes;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_environment.IsEnvironment("Testing")) return;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        var nextSweepAt = DateTime.MinValue;
        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<VideoTranscodeProcessor>();
                await processor.RunOnceAsync(stoppingToken);
                if (DateTime.UtcNow >= nextSweepAt)
                {
                    await processor.SweepAsync(stoppingToken);
                    nextSweepAt = DateTime.UtcNow.AddHours(1);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "Video transcode worker failed with {ExceptionType}.", exception.GetType().Name);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

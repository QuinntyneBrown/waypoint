using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Waypoint.Application;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Any(argument => argument is "--help" or "-h" or "-?" or "--version"))
        {
            return CliApplication.CreateRootCommand().Parse(args).Invoke();
        }

        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        try
        {
            InfrastructureStartupValidator.Validate(
                builder.Configuration,
                builder.Environment.EnvironmentName);
        }
        catch (InvalidOperationException exception)
        {
            Console.Error.WriteLine($"error: {exception.Message}");
            return 2;
        }
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddSingleton<IChangePublisher, OperationalChangePublisher>();
        builder.Services.RemoveAll<ICurrentUser>();
        builder.Services.AddSingleton<OperationalCurrentUser>();
        builder.Services.AddSingleton<ICurrentUser>(services =>
            services.GetRequiredService<OperationalCurrentUser>());

        using var host = builder.Build();
        return await CliApplication.RunAsync(
            args,
            host.Services,
            Console.Out,
            Console.Error);
    }
}

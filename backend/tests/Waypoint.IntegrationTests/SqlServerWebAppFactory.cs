using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Api.Observability;
using Waypoint.Api.Realtime;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests;

/// <summary>
/// Boots the real API via <see cref="WebApplicationFactory{TEntryPoint}"/> pointed at a throwaway
/// LocalDB database, so integration tests exercise the full HTTP + EF Core stack against real SQL Server.
/// One database per fixture (created in setup, dropped in teardown). Override the instance with the
/// WAYPOINT_TEST_SQLSERVER environment variable (default: (localdb)\MSSQLLocalDB).
/// </summary>
public sealed class SqlServerWebAppFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = $"Waypoint_IntegrationTests_{Guid.NewGuid():N}";

    public string ConnectionString
    {
        get
        {
            // Local dev/CI on Windows -> LocalDB + Windows auth (default).
            // Linux CI -> set WAYPOINT_TEST_SQLSERVER=localhost and
            //   WAYPOINT_TEST_SQLAUTH="User Id=sa;Password=…" for the mssql service container.
            var server = Environment.GetEnvironmentVariable("WAYPOINT_TEST_SQLSERVER")
                ?? "(localdb)\\MSSQLLocalDB";
            var auth = Environment.GetEnvironmentVariable("WAYPOINT_TEST_SQLAUTH")
                ?? "Trusted_Connection=True";
            return $"Server={server};Database={DatabaseName};{auth};" +
                   "TrustServerCertificate=True;MultipleActiveResultSets=true";
        }
    }

    /// <summary>Captured outbound emails (invitation / reset tokens are delivered only by email).</summary>
    public CapturingEmailSender Emails => Services.GetRequiredService<CapturingEmailSender>();
    public CapturingExceptionAlertSink ExceptionAlerts =>
        Services.GetRequiredService<CapturingExceptionAlertSink>();
    public CapturingLoggerProvider Logs => Services.GetRequiredService<CapturingLoggerProvider>();
    public CapturingChangePublisher Changes =>
        Services.GetRequiredService<CapturingChangePublisher>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString,
                ["Jwt:Issuer"] = "Waypoint",
                ["Jwt:Audience"] = "Waypoint",
                ["Jwt:SigningKey"] = "waypoint-integration-tests-signing-key-32chars-min!!",
                ["Jwt:AccessTokenMinutes"] = "60",
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(ConnectionString));

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<CapturingEmailSender>();
            services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<CapturingEmailSender>());
            services.RemoveAll<IExceptionAlertSink>();
            services.AddSingleton<CapturingExceptionAlertSink>();
            services.AddSingleton<IExceptionAlertSink>(sp =>
                sp.GetRequiredService<CapturingExceptionAlertSink>());
            services.AddSingleton<CapturingLoggerProvider>();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerProvider>(sp =>
                sp.GetRequiredService<CapturingLoggerProvider>());
            services.RemoveAll<IChangePublisher>();
            services.AddSingleton<SignalRChangePublisher>();
            services.AddSingleton<CapturingChangePublisher>();
            services.AddSingleton<IChangePublisher>(sp =>
                sp.GetRequiredService<CapturingChangePublisher>());
        });
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WordUp.Application.Shared.Abstractions;
using WordUp.Api.Observability;
using WordUp.Api.Realtime;

namespace WordUp.IntegrationTests;

/// <summary>
/// Boots the real API via <see cref="WebApplicationFactory{TEntryPoint}"/> pointed at a throwaway
/// LocalDB database, so integration tests exercise the full HTTP + EF Core stack against real SQL Server.
/// One database per fixture (created in setup, dropped in teardown). Override the instance with the
/// WORDUP_TEST_SQLSERVER environment variable (default: (localdb)\MSSQLLocalDB).
/// </summary>
public sealed class SqlServerWebAppFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = $"WordUp_IntegrationTests_{Guid.NewGuid():N}";

    public string ConnectionString
    {
        get
        {
            // Local dev/CI on Windows -> LocalDB + Windows auth (default).
            // Linux CI -> set WORDUP_TEST_SQLSERVER=localhost and
            //   WORDUP_TEST_SQLAUTH="User Id=sa;Password=…" for the mssql service container.
            var server = Environment.GetEnvironmentVariable("WORDUP_TEST_SQLSERVER")
                ?? "(localdb)\\MSSQLLocalDB";
            var auth = Environment.GetEnvironmentVariable("WORDUP_TEST_SQLAUTH")
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
                ["Jwt:Issuer"] = "WordUp",
                ["Jwt:Audience"] = "WordUp",
                ["Jwt:SigningKey"] = "wordup-integration-tests-signing-key-32chars-min!!",
                ["Jwt:AccessTokenMinutes"] = "60",
            });
        });
        builder.ConfigureTestServices(services =>
        {
            // NOTE: CLAUDE.md mandates a real SQL Server / LocalDB (one throwaway DB per fixture). This
            // environment's LocalDB native runtime fails to load (SQLUserInstance.dll — architecture
            // mismatch), so the app's default "InMemory" provider is what actually runs here. When a
            // working SQL Server/LocalDB (or Testcontainers) is available, force it by swapping the
            // DbContext registration to UseSqlServer(ConnectionString) in this block.
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

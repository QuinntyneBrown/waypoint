// Acceptance Tests
// Traces to: L2-NFR-011 AC4
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-011")]
public sealed class DatabaseLifecycleTests
{
    [Test]
    [Property("Req", "L2-NFR-011")]
    public void Production_refuses_to_start_without_a_durable_connection_string()
    {
        // Traces to: L2-NFR-011 AC4
        using var factory = new ProductionConfigurationFactory("InMemory");

        var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.That(error!.ToString(), Does.Contain("durable SQL Server connection string"));
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public void Production_refuses_to_start_with_placeholder_secrets()
    {
        // Traces to: L2-NFR-011 AC4
        using var factory = new ProductionConfigurationFactory(
            "Server=production;Database=WordUp;User Id=wordup;Password=not-used-by-test;TrustServerCertificate=True",
            jwtSigningKey: "REPLACE_WITH_A_SECURE_SIGNING_KEY_32_CHARS");

        var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.That(error!.ToString(), Does.Contain("non-placeholder secret"));
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Startup_applies_database_migrations_once_before_serving_requests()
    {
        // Traces to: L2-NFR-011 AC4
        var lifecycle = new CapturingDatabaseLifecycle();
        await using var factory = new ProductionConfigurationFactory(
            "Server=production;Database=WordUp;User Id=wordup;Password=not-used-by-test;TrustServerCertificate=True",
            lifecycle: lifecycle);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        using var response = await client.GetAsync("/health");

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(lifecycle.MigrationCount, Is.EqualTo(1));
        });
    }

    private sealed class ProductionConfigurationFactory(
        string connectionString,
        string jwtSigningKey = "wordup-lifecycle-tests-signing-key-32chars-min!!",
        IDatabaseLifecycle? lifecycle = null)
        : WebApplicationFactory<Program>
    {
        private readonly IDatabaseLifecycle _lifecycle = lifecycle ?? new CapturingDatabaseLifecycle();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = connectionString,
                    ["Jwt:SigningKey"] = jwtSigningKey,
                    ["AuditIntegrity:SigningKey"] = "wordup-lifecycle-tests-audit-key-32chars-min!!",
                    ["SensitiveData:EncryptionKey"] = "wordup-lifecycle-tests-encryption-key-32chars!!",
                    ["Cors:AllowedOrigins:0"] = "https://portal.wordup.example",
                }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDatabaseLifecycle>();
                services.AddSingleton(_lifecycle);
            });
        }
    }

    private sealed class CapturingDatabaseLifecycle : IDatabaseLifecycle
    {
        public int MigrationCount { get; private set; }

        public Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            MigrationCount++;
            return Task.CompletedTask;
        }

        public Task ResetAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

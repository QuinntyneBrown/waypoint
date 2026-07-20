// Acceptance Tests
// Traces to: L2-NFR-011 AC4
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-011")]
public sealed class DatabaseLifecycleTests
{
    [Test]
    public async Task Fresh_sql_database_applies_the_real_migration_chain_and_accepts_completion_rows()
    {
        await using var factory = new SqlServerWebAppFactory();
        try
        {
            using var client = factory.CreateClient();
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(
                db.Database.ProviderName,
                Is.EqualTo("Microsoft.EntityFrameworkCore.SqlServer"));

            var now = DateTime.UtcNow;
            var subject = new User
            {
                Email = $"migration.user.{Guid.NewGuid():N}@example.com",
                PasswordHash = "migration-smoke-hash",
                DisplayName = "Migration subject",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CreatedAt = now,
            };
            db.Users.Add(subject);
            db.Invitations.Add(new Invitation
            {
                Email = $"migration.{Guid.NewGuid():N}@example.com",
                Role = Roles.Youth,
                TokenHash = Guid.NewGuid().ToString("N"),
                ExpiresAt = now.AddDays(14),
            });
            db.DataSubjectRequests.Add(new DataSubjectRequest
            {
                Reference = $"DSR-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
                SubjectUserId = subject.Id,
                ActionType = "correction",
                RequestedClearPronouns = true,
                RequestedAt = now,
            });
            db.OutboundEmails.Add(new OutboundEmail
            {
                ToEmail = $"migration.outbox.{Guid.NewGuid():N}@example.com",
                Subject = "Migration smoke",
                Body = "Migration smoke body",
                RecipientUserId = subject.Id,
                AvailableAt = now.AddDays(1),
            });
            db.ContactRequests.Add(new ContactRequest
            {
                OwnerUserId = subject.Id,
                Name = "Migration subject",
                Email = subject.Email,
                Topic = "Migration smoke",
                Message = "Migration smoke body",
                CreatedAt = now,
            });

            await db.SaveChangesAsync();
            var applied = await db.Database.GetAppliedMigrationsAsync();

            Assert.Multiple(() =>
            {
                Assert.That(applied.Any(item =>
                    item.EndsWith("_CompleteInvitationEmailPrivacyDelivery", StringComparison.Ordinal)), Is.True);
                Assert.That(applied.Any(item =>
                    item.EndsWith("_SerializePrivacyDeliveryOwnership", StringComparison.Ordinal)), Is.True);
                Assert.That(applied.Any(item =>
                    item.EndsWith("_EnforcePrivacyOwnershipIntegrity", StringComparison.Ordinal)), Is.True);
                Assert.That(db.Invitations.Count(), Is.EqualTo(1));
                Assert.That(db.DataSubjectRequests.Count(), Is.EqualTo(1));
                Assert.That(db.OutboundEmails.Count(), Is.EqualTo(1));
                Assert.That(db.ContactRequests.Count(), Is.EqualTo(1));
            });
        }
        finally
        {
            using var cleanupScope = factory.Services.CreateScope();
            var cleanup = cleanupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [Test]
    public async Task Ownership_migration_backfills_guardian_from_existing_authoritative_relationship()
    {
        const string priorMigration = "20260720064526_CompleteInvitationEmailPrivacyDelivery";
        await using var factory = new SqlServerWebAppFactory();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(factory.ConnectionString)
            .Options;
        var protector = new AesSensitiveDataProtector(Options.Create(new SensitiveDataOptions
        {
            EncryptionKey = "wordup-migration-backfill-key-32chars-minimum!!",
        }));
        await using var db = new AppDbContext(options, protector);
        try
        {
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync(priorMigration);

            var now = DateTime.UtcNow;
            var guardianId = Guid.NewGuid();
            var youthId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO Users
                    (Id, Email, PasswordHash, DisplayName, Role, Status, SessionVersion,
                     OnboardingCompleted, GuardianUserId, FailedSignInAttempts, CreatedAt)
                VALUES
                    ({guardianId}, {"guardian.backfill@example.com"}, {"hash"}, {"Guardian"},
                     {Roles.Guardian}, {(int)UserStatus.Active}, {0}, {true}, {(Guid?)null}, {0}, {now}),
                    ({youthId}, {"youth.backfill@example.com"}, {"hash"}, {"Youth"},
                     {Roles.Youth}, {(int)UserStatus.Active}, {0}, {true}, {guardianId}, {0}, {now});

                INSERT INTO Applications
                    (Id, FirstName, LastName, Age, WhoIsFilling, Neighbourhood,
                     CohortInterest, WhyInterested, ParticipationConsentAck, PrivacyAck,
                     Status, CreatedYouthUserId, CreatedAt)
                VALUES
                    ({applicationId}, {"Youth"}, {"Applicant"}, {15}, {"guardian"},
                     {"Neighbourhood"}, {"Next cohort"}, {"Interested"}, {true}, {true},
                     {(int)ApplicationStatus.Approved}, {youthId}, {now});
                """);

            await migrator.MigrateAsync();
            db.ChangeTracker.Clear();

            var application = await db.Applications.AsNoTracking()
                .SingleAsync(item => item.Id == applicationId);
            var applied = await db.Database.GetAppliedMigrationsAsync();
            Assert.Multiple(() =>
            {
                Assert.That(application.GuardianUserId, Is.EqualTo(guardianId));
                Assert.That(application.ApplicantUserId, Is.Null,
                    "the migration must not infer applicant ownership from submitted content");
                Assert.That(applied.Last(), Does.EndWith("_EnforcePrivacyOwnershipIntegrity"));
            });
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    [Test]
    public void Production_refuses_insecure_application_links_and_disabled_smtp()
    {
        var insecureLinks = ProductionSettings();
        insecureLinks["ApplicationUrls:PortalBaseUrl"] = "http://portal.wordup.example";
        var insecureConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(insecureLinks)
            .Build();
        var linkError = Assert.Throws<InvalidOperationException>(() =>
            InfrastructureStartupValidator.Validate(insecureConfiguration, "Production"));
        Assert.That(linkError!.Message, Does.Contain("absolute HTTPS URL"));

        var smtpDisabled = ProductionSettings();
        smtpDisabled["Smtp:Enabled"] = "false";
        var smtpConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(smtpDisabled)
            .Build();
        var smtpError = Assert.Throws<InvalidOperationException>(() =>
            InfrastructureStartupValidator.Validate(smtpConfiguration, "Production"));
        Assert.That(smtpError!.Message, Does.Contain("SMTP delivery"));
    }

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
                    ["ApplicationUrls:PortalBaseUrl"] = "https://portal.wordup.example",
                    ["ApplicationUrls:PublicSiteBaseUrl"] = "https://wordup.example",
                    ["Smtp:Enabled"] = "true",
                    ["Smtp:Host"] = "smtp.wordup.example",
                    ["Smtp:Port"] = "587",
                    ["Smtp:UseStartTls"] = "true",
                    ["Smtp:FromEmail"] = "noreply@wordup.example",
                }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDatabaseLifecycle>();
                services.AddSingleton(_lifecycle);
            });
        }
    }

    private static Dictionary<string, string?> ProductionSettings() => new()
    {
        ["ConnectionStrings:Default"] =
            "Server=production;Database=WordUp;User Id=wordup;Password=not-used-by-test;TrustServerCertificate=True",
        ["Jwt:SigningKey"] = "wordup-production-settings-jwt-key-32chars!!",
        ["AuditIntegrity:SigningKey"] = "wordup-production-settings-audit-key-32chars!!",
        ["SensitiveData:EncryptionKey"] = "wordup-production-settings-encryption-key-32chars!!",
        ["ApplicationUrls:PortalBaseUrl"] = "https://portal.wordup.example",
        ["ApplicationUrls:PublicSiteBaseUrl"] = "https://wordup.example",
        ["Smtp:Enabled"] = "true",
        ["Smtp:Host"] = "smtp.wordup.example",
        ["Smtp:Port"] = "587",
        ["Smtp:UseStartTls"] = "true",
        ["Smtp:FromEmail"] = "noreply@wordup.example",
    };

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

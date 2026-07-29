// Acceptance Tests
// Traces to: L2-NFR-011 AC3
using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WordUp.Application;
using WordUp.Application.Shared.Abstractions;
using WordUp.Cli;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-011")]
public sealed class OperationalCliTests
{
    [Test]
    [Property("Req", "L2-NFR-011")]
    public void Required_operational_command_tree_is_available()
    {
        // Traces to: L2-NFR-011 AC3
        var root = CliApplication.CreateRootCommand();

        Assert.Multiple(() =>
        {
            AssertCommands(root, "db", "migrate", "seed", "reset");
            AssertCommands(
                root,
                "user",
                "create",
                "list",
                "set-role",
                "link-guardian",
                "reset-password",
                "deactivate");
            AssertCommands(root, "invite", "create", "resend", "revoke", "list");
            AssertCommands(root, "cohort", "create", "assign-mentor", "add-youth", "progress");
            AssertCommands(root, "import", "youth");
            AssertCommands(root, "report", "youth", "cohort");
            AssertCommands(root, "badge", "award");
            AssertCommands(root, "token", "issue");
        });
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Badge_award_uses_the_same_application_handler_and_audit_path_as_the_api()
    {
        // Traces to: L2-NFR-011 AC3
        await using var services = BuildServices();
        var admin = new User
        {
            Email = "operator@example.com",
            DisplayName = "Operator",
            Role = Roles.ProgramAdministrator,
            Status = UserStatus.Active,
        };
        var youth = new User
        {
            Email = "youth@example.com",
            DisplayName = "Youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
        };
        var badge = new Badge
        {
            Name = "Operational award",
            Icon = "star",
            CriteriaKind = BadgeCriteriaKind.Manual,
        };
        await using (var seedScope = services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.Users.AddRange(admin, youth);
            db.Badges.Add(badge);
            await db.SaveChangesAsync();
        }
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(
            ["badge", "award", youth.Id.ToString(), badge.Id.ToString(), "--actor", admin.Id.ToString()],
            services,
            output,
            error);

        await using var assertionScope = services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.Zero, error.ToString());
            Assert.That(assertionDb.BadgeAwards.Any(
                award => award.YouthUserId == youth.Id && award.BadgeId == badge.Id), Is.True);
            Assert.That(assertionDb.AuditEntries.Any(
                entry => entry.Action == "BadgeAwarded" && entry.ActorUserId == admin.Id), Is.True);
        });
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task User_create_enforces_the_application_password_policy_and_audits_success()
    {
        // Traces to: L2-NFR-011 AC3
        await using var services = BuildServices();
        var admin = new User
        {
            Email = "user-operator@example.com",
            DisplayName = "User operator",
            Role = Roles.ProgramAdministrator,
            Status = UserStatus.Active,
        };
        await using (var seedScope = services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new[]
        {
            "user", "create",
            "--email", "created@example.com",
            "--display-name", "Created user",
            "--role", Roles.Youth,
            "--actor", admin.Id.ToString(),
            "--password",
        };

        var rejected = await CliApplication.RunAsync(
            [.. command, "weak"], services, output, error);
        var accepted = await CliApplication.RunAsync(
            [.. command, "Passw0rd!"], services, output, error);

        await using var assertionScope = services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(rejected, Is.EqualTo(2));
            Assert.That(error.ToString(), Does.Contain("Password must be at least 8 characters"));
            Assert.That(accepted, Is.Zero, error.ToString());
            Assert.That(assertionDb.Users.Count(user => user.Email == "created@example.com"), Is.EqualTo(1));
            Assert.That(assertionDb.AuditEntries.Any(entry =>
                entry.Action == "UserCreated" && entry.ActorUserId == admin.Id), Is.True);
        });
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Guardian_link_records_the_relationship_and_audits_the_operator()
    {
        // Traces to: L2-NFR-011 AC3
        await using var services = BuildServices();
        var admin = new User
        {
            Email = "link-operator@example.com",
            DisplayName = "Link operator",
            Role = Roles.ProgramAdministrator,
            Status = UserStatus.Active,
        };
        var guardian = new User
        {
            Email = "guardian@example.com",
            DisplayName = "Guardian",
            Role = Roles.Guardian,
            Status = UserStatus.Active,
        };
        var youth = new User
        {
            Email = "linked-youth@example.com",
            DisplayName = "Linked youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
        };
        await using (var seedScope = services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.Users.AddRange(admin, guardian, youth);
            await db.SaveChangesAsync();
        }
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(
            [
                "user", "link-guardian",
                youth.Id.ToString(),
                guardian.Id.ToString(),
                "--actor", admin.Id.ToString(),
            ],
            services,
            output,
            error);

        await using var assertionScope = services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.Zero, error.ToString());
            Assert.That(
                assertionDb.Users.Single(user => user.Id == youth.Id).GuardianUserId,
                Is.EqualTo(guardian.Id));
            Assert.That(assertionDb.AuditEntries.Any(entry =>
                entry.Action == "GuardianLinked"
                && entry.ActorUserId == admin.Id
                && entry.TargetId == youth.Id
                && entry.Details == guardian.Id.ToString()), Is.True);
        });
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Guardian_link_refuses_a_target_that_is_not_an_active_guardian()
    {
        // Traces to: L2-NFR-011 AC3
        await using var services = BuildServices();
        var admin = new User
        {
            Email = "link-refusal-operator@example.com",
            DisplayName = "Link refusal operator",
            Role = Roles.ProgramAdministrator,
            Status = UserStatus.Active,
        };
        var mentor = new User
        {
            Email = "not-a-guardian@example.com",
            DisplayName = "Mentor",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        var youth = new User
        {
            Email = "unlinked-youth@example.com",
            DisplayName = "Unlinked youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
        };
        await using (var seedScope = services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.Users.AddRange(admin, mentor, youth);
            await db.SaveChangesAsync();
        }
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(
            [
                "user", "link-guardian",
                youth.Id.ToString(),
                mentor.Id.ToString(),
                "--actor", admin.Id.ToString(),
            ],
            services,
            output,
            error);

        await using var assertionScope = services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(2));
            Assert.That(error.ToString(), Does.Contain("active guardian"));
            Assert.That(
                assertionDb.Users.Single(user => user.Id == youth.Id).GuardianUserId,
                Is.Null);
            Assert.That(
                assertionDb.AuditEntries.Any(entry =>
                    entry.Action == "GuardianLinked" && entry.TargetId == youth.Id),
                Is.False);
        });
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Database_reset_requires_an_exact_confirmation_before_invoking_the_lifecycle()
    {
        // Traces to: L2-NFR-011 AC3
        var lifecycle = new CapturingDatabaseLifecycle();
        await using var services = BuildServices(lifecycle);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var rejected = await CliApplication.RunAsync(
            ["db", "reset", "--confirm", "reset"], services, output, error);
        var accepted = await CliApplication.RunAsync(
            ["db", "reset", "--confirm", "RESET"], services, output, error);

        Assert.Multiple(() =>
        {
            Assert.That(rejected, Is.EqualTo(2));
            Assert.That(accepted, Is.Zero, error.ToString());
            Assert.That(lifecycle.ResetCount, Is.EqualTo(1));
        });
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Database_seed_migrates_and_idempotently_creates_the_initial_administrator()
    {
        // Traces to: L2-NFR-011 AC3
        var lifecycle = new CapturingDatabaseLifecycle();
        await using var services = BuildServices(lifecycle);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new[]
        {
            "db", "seed",
            "--email", "initial-admin@example.com",
            "--password", "Passw0rd!",
            "--display-name", "Initial administrator",
        };

        var first = await CliApplication.RunAsync(command, services, output, error);
        var second = await CliApplication.RunAsync(command, services, output, error);

        await using var assertionScope = services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Zero, error.ToString());
            Assert.That(second, Is.Zero, error.ToString());
            Assert.That(lifecycle.MigrationCount, Is.EqualTo(2));
            Assert.That(assertionDb.Users.Count(user =>
                user.Email == "initial-admin@example.com"
                && user.Role == Roles.ProgramAdministrator), Is.EqualTo(1));
        });
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Token_issue_uses_the_shared_issuer_and_records_the_operator_without_auditing_the_token()
    {
        // Traces to: L2-NFR-011 AC3
        await using var services = BuildServices();
        var admin = new User
        {
            Email = "token-operator@example.com",
            DisplayName = "Token operator",
            Role = Roles.ProgramAdministrator,
            Status = UserStatus.Active,
        };
        var user = new User
        {
            Email = "token-user@example.com",
            DisplayName = "Token user",
            Role = Roles.Volunteer,
            Status = UserStatus.Active,
        };
        await using (var seedScope = services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.Users.AddRange(admin, user);
            await db.SaveChangesAsync();
        }
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(
            ["token", "issue", "--user", user.Email, "--actor", admin.Id.ToString()],
            services,
            output,
            error);

        await using var assertionScope = services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var audit = assertionDb.AuditEntries.Single(entry =>
            entry.Action == "OperationalTokenIssued" && entry.TargetId == user.Id);
        using var result = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.Zero, error.ToString());
            Assert.That(
                result.RootElement.GetProperty("accessToken").GetString()!.Split('.'),
                Has.Length.EqualTo(3));
            Assert.That(audit.ActorUserId, Is.EqualTo(admin.Id));
            Assert.That(audit.Details, Is.Null);
        });
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Invitation_list_exposes_operational_state_without_token_hashes()
    {
        // Traces to: L2-NFR-011 AC3
        await using var services = BuildServices();
        var admin = new User
        {
            Email = "invite-operator@example.com",
            DisplayName = "Invite operator",
            Role = Roles.ProgramAdministrator,
            Status = UserStatus.Active,
        };
        var invitation = new Invitation
        {
            Email = "invitee@example.com",
            Role = Roles.Volunteer,
            Status = InvitationStatus.Pending,
            TokenHash = "must-never-appear-in-cli-output",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
        };
        await using (var seedScope = services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(admin);
            db.Invitations.Add(invitation);
            await db.SaveChangesAsync();
        }
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(
            ["invite", "list", "--actor", admin.Id.ToString()],
            services,
            output,
            error);

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.Zero, error.ToString());
            Assert.That(output.ToString(), Does.Contain(invitation.Email));
            Assert.That(output.ToString(), Does.Not.Contain(invitation.TokenHash));
            Assert.That(output.ToString(), Does.Not.Contain("tokenHash").IgnoreCase);
        });
    }

    private static void AssertCommands(RootCommand root, string groupName, params string[] expected)
    {
        var group = root.Subcommands.SingleOrDefault(command => command.Name == groupName);
        Assert.That(group, Is.Not.Null, $"Missing '{groupName}' command group.");
        Assert.That(
            group!.Subcommands.Select(command => command.Name),
            Is.EquivalentTo(expected),
            $"Unexpected '{groupName}' command surface.");
    }

    private static ServiceProvider BuildServices(IDatabaseLifecycle? lifecycle = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "InMemory",
                ["Jwt:Issuer"] = "WordUp",
                ["Jwt:Audience"] = "WordUp",
                ["Jwt:SigningKey"] = "wordup-cli-tests-signing-key-32-characters!!",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["AuditIntegrity:SigningKey"] = "wordup-cli-tests-audit-key-32-characters!!",
                ["SensitiveData:EncryptionKey"] = "wordup-cli-tests-encryption-key-32-characters!!",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        if (lifecycle is not null)
        {
            services.RemoveAll<IDatabaseLifecycle>();
            services.AddSingleton(lifecycle);
        }
        services.RemoveAll<ICurrentUser>();
        services.AddSingleton<OperationalCurrentUser>();
        services.AddSingleton<ICurrentUser>(provider =>
            provider.GetRequiredService<OperationalCurrentUser>());
        return services.BuildServiceProvider();
    }

    private sealed class CapturingDatabaseLifecycle : IDatabaseLifecycle
    {
        public int MigrationCount { get; private set; }
        public int ResetCount { get; private set; }

        public Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            MigrationCount++;
            return Task.CompletedTask;
        }

        public Task ResetAsync(CancellationToken cancellationToken = default)
        {
            ResetCount++;
            return Task.CompletedTask;
        }
    }
}

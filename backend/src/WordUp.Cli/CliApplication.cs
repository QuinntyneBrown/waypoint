using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WordUp.Application.Authentication.Invitations;
using WordUp.Application.CurriculumAuthoring.Badges;
using WordUp.Application.OperationalAdministration;
using WordUp.Application.ProgramAdministration.Cohorts;
using WordUp.Application.ProgramAdministration.MentorAssignments;
using WordUp.Application.ProgramAdministration.People;
using WordUp.Application.ReportingAndData.DataTransfer;
using WordUp.Application.ReportingAndData.Reports;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;
using WordUp.Domain.Authentication;
using WordUp.Infrastructure;

namespace WordUp.Cli;

public static class CliApplication
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static RootCommand CreateRootCommand() => CreateRootCommand(runtime: null);

    public static Task<int> RunAsync(
        string[] args,
        IServiceProvider services,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken = default)
    {
        var configuration = new InvocationConfiguration
        {
            Output = output,
            Error = error,
        };
        var runtime = new CliRuntime(services, output, error);
        return CreateRootCommand(runtime).Parse(args).InvokeAsync(configuration, cancellationToken);
    }

    private static RootCommand CreateRootCommand(CliRuntime? runtime)
    {
        var root = new RootCommand("Word Up operational administration command line");
        var actor = new Option<Guid?>("--actor")
        {
            Description = "Active Program Administrator user id used for audit attribution",
            Recursive = true,
        };
        root.Options.Add(actor);
        root.Subcommands.Add(BuildDatabaseCommands(runtime, actor));
        root.Subcommands.Add(BuildUserCommands(runtime, actor));
        root.Subcommands.Add(BuildInvitationCommands(runtime, actor));
        root.Subcommands.Add(BuildCohortCommands(runtime, actor));
        root.Subcommands.Add(BuildImportCommands(runtime, actor));
        root.Subcommands.Add(BuildReportCommands(runtime, actor));
        root.Subcommands.Add(BuildBadgeCommands(runtime, actor));
        root.Subcommands.Add(BuildTokenCommands(runtime, actor));
        return root;
    }

    private static Command BuildDatabaseCommands(CliRuntime? runtime, Option<Guid?> actor)
    {
        var group = new Command("db", "Manage the Word Up database schema and seed data");

        var migrate = new Command("migrate", "Apply all pending database migrations");
        SetAction(migrate, runtime, actor, requiresActor: false, async (_, services, cancellationToken) =>
        {
            await services.GetRequiredService<IDatabaseLifecycle>().MigrateAsync(cancellationToken);
            await runtime!.Output.WriteLineAsync("Database migrations applied.");
        });

        var seed = new Command("seed", "Create an initial Program Administrator if it does not exist");
        var seedEmail = RequiredOption<string>("--email", "Initial administrator email");
        var seedPassword = RequiredOption<string>("--password", "Initial administrator password");
        var seedName = RequiredOption<string>("--display-name", "Initial administrator display name");
        seed.Options.Add(seedEmail);
        seed.Options.Add(seedPassword);
        seed.Options.Add(seedName);
        SetAction(seed, runtime, actor, requiresActor: false, async (result, services, cancellationToken) =>
        {
            await services.GetRequiredService<IDatabaseLifecycle>().MigrateAsync(cancellationToken);
            var sender = services.GetRequiredService<ISender>();
            var email = result.GetRequiredValue(seedEmail);
            var existing = await sender.Send(
                new ListPeopleQuery(PageSize: 500, Search: email),
                cancellationToken);
            var user = existing.Items.FirstOrDefault(item =>
                item.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
            if (user is not null)
            {
                await WriteJsonAsync(runtime!.Output, user);
                return;
            }

            var created = await sender.Send(new CreateOperationalUserCommand(
                email,
                result.GetRequiredValue(seedPassword),
                result.GetRequiredValue(seedName),
                Roles.ProgramAdministrator), cancellationToken);
            await WriteJsonAsync(runtime!.Output, created);
        });

        var reset = new Command("reset", "Delete and recreate the configured database");
        var confirmation = RequiredOption<string>("--confirm", "Must be exactly RESET");
        reset.Options.Add(confirmation);
        SetAction(reset, runtime, actor, requiresActor: false, async (result, services, cancellationToken) =>
        {
            if (!result.GetRequiredValue(confirmation).Equals("RESET", StringComparison.Ordinal))
            {
                throw new ValidationException("Database reset requires --confirm RESET.");
            }

            await services.GetRequiredService<IDatabaseLifecycle>().ResetAsync(cancellationToken);
            await runtime!.Output.WriteLineAsync("Database reset completed.");
        });

        group.Subcommands.Add(migrate);
        group.Subcommands.Add(seed);
        group.Subcommands.Add(reset);
        return group;
    }

    private static Command BuildUserCommands(CliRuntime? runtime, Option<Guid?> actor)
    {
        var group = new Command("user", "Manage Word Up user accounts");

        var create = new Command("create", "Create an active user account");
        var email = RequiredOption<string>("--email", "User email");
        var password = RequiredOption<string>("--password", "Initial password");
        var displayName = RequiredOption<string>("--display-name", "Display name");
        var role = RequiredOption<string>("--role", "Canonical Word Up role");
        var cohort = new Option<Guid?>("--cohort") { Description = "Optional cohort id" };
        var mentor = new Option<Guid?>("--mentor") { Description = "Optional mentor user id" };
        create.Options.Add(email);
        create.Options.Add(password);
        create.Options.Add(displayName);
        create.Options.Add(role);
        create.Options.Add(cohort);
        create.Options.Add(mentor);
        SetAction(create, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            var created = await services.GetRequiredService<ISender>().Send(
                new CreateOperationalUserCommand(
                    result.GetRequiredValue(email),
                    result.GetRequiredValue(password),
                    result.GetRequiredValue(displayName),
                    result.GetRequiredValue(role),
                    result.GetValue(cohort),
                    result.GetValue(mentor)),
                cancellationToken);
            await WriteJsonAsync(runtime!.Output, created);
        });

        var list = new Command("list", "List non-removed users");
        var listRole = new Option<string?>("--role") { Description = "Filter by role" };
        var search = new Option<string?>("--search") { Description = "Search email or display name" };
        list.Options.Add(listRole);
        list.Options.Add(search);
        SetAction(list, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            var users = await services.GetRequiredService<ISender>().Send(
                new ListPeopleQuery(
                    PageSize: 500,
                    Role: result.GetValue(listRole),
                    Search: result.GetValue(search)),
                cancellationToken);
            await WriteJsonAsync(runtime!.Output, users);
        });

        var setRole = new Command("set-role", "Change a user's role and revoke existing sessions");
        var setRoleUserId = RequiredArgument<Guid>("id", "User id");
        var newRole = RequiredArgument<string>("role", "Canonical Word Up role");
        setRole.Arguments.Add(setRoleUserId);
        setRole.Arguments.Add(newRole);
        SetSenderAction(setRole, runtime, actor, result =>
            new ChangeRoleCommand(
                result.GetRequiredValue(setRoleUserId),
                result.GetRequiredValue(newRole)));

        var linkGuardian = new Command(
            "link-guardian",
            "Link an active guardian to an active youth");
        var linkYouthId = RequiredArgument<Guid>("youth-id", "Youth user id");
        var linkGuardianId = RequiredArgument<Guid>("guardian-id", "Guardian user id");
        linkGuardian.Arguments.Add(linkYouthId);
        linkGuardian.Arguments.Add(linkGuardianId);
        SetSenderAction(linkGuardian, runtime, actor, result =>
            new LinkGuardianCommand(
                result.GetRequiredValue(linkYouthId),
                result.GetRequiredValue(linkGuardianId)));

        var resetPassword = new Command(
            "reset-password",
            "Send the user a single-use password reset link");
        var resetUserId = RequiredArgument<Guid>("id", "User id");
        resetPassword.Arguments.Add(resetUserId);
        SetSenderAction(resetPassword, runtime, actor, result =>
            new SendPasswordResetForPersonCommand(result.GetRequiredValue(resetUserId)));

        var deactivate = new Command("deactivate", "Deactivate a user and revoke existing sessions");
        var deactivateUserId = RequiredArgument<Guid>("id", "User id");
        deactivate.Arguments.Add(deactivateUserId);
        SetSenderAction(deactivate, runtime, actor, result =>
            new DeactivatePersonCommand(result.GetRequiredValue(deactivateUserId)));

        group.Subcommands.Add(create);
        group.Subcommands.Add(list);
        group.Subcommands.Add(setRole);
        group.Subcommands.Add(linkGuardian);
        group.Subcommands.Add(resetPassword);
        group.Subcommands.Add(deactivate);
        return group;
    }

    private static Command BuildInvitationCommands(CliRuntime? runtime, Option<Guid?> actor)
    {
        var group = new Command("invite", "Manage account invitations");

        var create = new Command("create", "Create and email an invitation");
        var email = RequiredOption<string>("--email", "Invitee email");
        var role = RequiredOption<string>("--role", "Canonical Word Up role");
        var cohort = new Option<Guid?>("--cohort") { Description = "Optional cohort id" };
        var mentor = new Option<Guid?>("--mentor") { Description = "Optional mentor user id" };
        create.Options.Add(email);
        create.Options.Add(role);
        create.Options.Add(cohort);
        create.Options.Add(mentor);
        SetAction(create, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            var invitation = await services.GetRequiredService<ISender>().Send(
                new InviteCommand(
                    result.GetRequiredValue(email),
                    result.GetRequiredValue(role),
                    result.GetValue(cohort),
                    result.GetValue(mentor)),
                cancellationToken);
            await WriteJsonAsync(runtime!.Output, invitation);
        });

        var resend = new Command("resend", "Replace and email an invitation token");
        var resendId = RequiredArgument<Guid>("id", "Invitation id");
        resend.Arguments.Add(resendId);
        SetSenderAction(resend, runtime, actor, result =>
            new ResendInvitationCommand(result.GetRequiredValue(resendId)));

        var revoke = new Command("revoke", "Revoke a pending invitation");
        var revokeId = RequiredArgument<Guid>("id", "Invitation id");
        revoke.Arguments.Add(revokeId);
        SetSenderAction(revoke, runtime, actor, result =>
            new RevokeInvitationCommand(result.GetRequiredValue(revokeId)));

        var list = new Command("list", "List invitations without exposing token hashes");
        SetAction(list, runtime, actor, requiresActor: true, async (_, services, cancellationToken) =>
        {
            var invitations = await services.GetRequiredService<ISender>()
                .Send(new ListInvitationsQuery(), cancellationToken);
            await WriteJsonAsync(runtime!.Output, invitations);
        });

        group.Subcommands.Add(create);
        group.Subcommands.Add(resend);
        group.Subcommands.Add(revoke);
        group.Subcommands.Add(list);
        return group;
    }

    private static Command BuildCohortCommands(CliRuntime? runtime, Option<Guid?> actor)
    {
        var group = new Command("cohort", "Manage cohorts and placements");

        var create = new Command("create", "Create a cohort");
        var name = RequiredOption<string>("--name", "Cohort name");
        var track = RequiredOption<Guid>("--track", "Published track id");
        var leadMentor = RequiredOption<Guid>("--lead-mentor", "Lead mentor user id");
        var start = new Option<DateTime?>("--start") { Description = "Optional start date" };
        var end = new Option<DateTime?>("--end") { Description = "Optional end date" };
        var location = new Option<string?>("--location") { Description = "Optional location" };
        create.Options.Add(name);
        create.Options.Add(track);
        create.Options.Add(leadMentor);
        create.Options.Add(start);
        create.Options.Add(end);
        create.Options.Add(location);
        SetAction(create, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            var cohort = await services.GetRequiredService<ISender>().Send(
                new CreateCohortCommand(
                    result.GetRequiredValue(name),
                    result.GetRequiredValue(track),
                    result.GetRequiredValue(leadMentor),
                    result.GetValue(start),
                    result.GetValue(end),
                    result.GetValue(location)),
                cancellationToken);
            await WriteJsonAsync(runtime!.Output, cohort);
        });

        var assignMentor = new Command("assign-mentor", "Set lead and supporting cohort mentors");
        var assignCohortId = RequiredArgument<Guid>("id", "Cohort id");
        var lead = RequiredOption<Guid>("--lead", "Lead mentor user id");
        var supporting = new Option<Guid[]>("--supporting")
        {
            Description = "Supporting mentor ids",
            AllowMultipleArgumentsPerToken = true,
        };
        assignMentor.Arguments.Add(assignCohortId);
        assignMentor.Options.Add(lead);
        assignMentor.Options.Add(supporting);
        SetSenderAction(assignMentor, runtime, actor, result =>
            new SetCohortMentorsCommand(
                result.GetRequiredValue(assignCohortId),
                result.GetRequiredValue(lead),
                result.GetValue(supporting) ?? []));

        var addYouth = new Command("add-youth", "Place a youth in a cohort");
        var addCohortId = RequiredArgument<Guid>("id", "Cohort id");
        var youthId = RequiredArgument<Guid>("youth-id", "Youth user id");
        addYouth.Arguments.Add(addCohortId);
        addYouth.Arguments.Add(youthId);
        SetSenderAction(addYouth, runtime, actor, result =>
            new AddYouthToCohortCommand(
                result.GetRequiredValue(addCohortId),
                result.GetRequiredValue(youthId)));

        var progress = new Command("progress", "Show the cohort progress board as JSON");
        var progressCohortId = RequiredArgument<Guid>("id", "Cohort id");
        var progressTrack = new Option<Guid?>("--track") { Description = "Optional track id" };
        progress.Arguments.Add(progressCohortId);
        progress.Options.Add(progressTrack);
        SetAction(progress, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            var board = await services.GetRequiredService<ISender>().Send(
                new ProgressBoardQuery(
                    result.GetRequiredValue(progressCohortId),
                    result.GetValue(progressTrack)),
                cancellationToken);
            await WriteJsonAsync(runtime!.Output, board);
        });

        group.Subcommands.Add(create);
        group.Subcommands.Add(assignMentor);
        group.Subcommands.Add(addYouth);
        group.Subcommands.Add(progress);
        return group;
    }

    private static Command BuildImportCommands(CliRuntime? runtime, Option<Guid?> actor)
    {
        var group = new Command("import", "Import operational data");
        var youth = new Command(
            "youth",
            "Import a CSV whose headers use Name, Email, Cohort, Mentor, Track, Status, and Consent");
        var source = RequiredOption<FileInfo>("--from", "CSV file path");
        source.AcceptExistingOnly();
        youth.Options.Add(source);
        SetAction(youth, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            var csv = await File.ReadAllTextAsync(result.GetRequiredValue(source).FullName, cancellationToken);
            var fields = new[] { "Name", "Email", "Cohort", "Mentor", "Track", "Status", "Consent" };
            var mapping = fields.ToDictionary(field => field, field => field, StringComparer.OrdinalIgnoreCase);
            var imported = await services.GetRequiredService<ISender>()
                .Send(new CommitRosterImportCommand(csv, mapping), cancellationToken);
            await WriteJsonAsync(runtime!.Output, imported);
        });
        group.Subcommands.Add(youth);
        return group;
    }

    private static Command BuildReportCommands(CliRuntime? runtime, Option<Guid?> actor)
    {
        var group = new Command("report", "Export youth and cohort reports");
        group.Subcommands.Add(BuildReportCommand("youth", runtime, actor, isYouth: true));
        group.Subcommands.Add(BuildReportCommand("cohort", runtime, actor, isYouth: false));
        return group;
    }

    private static Command BuildReportCommand(
        string name,
        CliRuntime? runtime,
        Option<Guid?> actor,
        bool isYouth)
    {
        var command = new Command(name, $"Export a {name} report");
        var targetId = RequiredArgument<Guid>("id", $"{name} id");
        var format = RequiredOption<string>("--format", "pdf or csv");
        format.AcceptOnlyFromAmong("pdf", "csv");
        var output = new Option<FileInfo?>("--out") { Description = "Output path" };
        command.Arguments.Add(targetId);
        command.Options.Add(format);
        command.Options.Add(output);
        SetAction(command, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            var id = result.GetRequiredValue(targetId);
            var export = await services.GetRequiredService<ISender>().Send(
                new ExportStandardReportQuery(
                    name,
                    result.GetRequiredValue(format),
                    new ReportFilter(CohortId: isYouth ? null : id),
                    YouthId: isYouth ? id : null,
                    CohortId: isYouth ? null : id),
                cancellationToken);
            var requestedOutput = result.GetValue(output);
            var path = requestedOutput?.FullName ?? Path.GetFullPath(export.FileName);
            if (File.Exists(path))
            {
                throw new IOException($"Output file already exists: {path}");
            }

            await File.WriteAllBytesAsync(path, export.Content, cancellationToken);
            await runtime!.Output.WriteLineAsync(path);
        });
        return command;
    }

    private static Command BuildBadgeCommands(CliRuntime? runtime, Option<Guid?> actor)
    {
        var group = new Command("badge", "Manage badge awards");
        var award = new Command("award", "Award a manual badge to an active youth");
        var youthId = RequiredArgument<Guid>("youthId", "Youth user id");
        var badgeId = RequiredArgument<Guid>("badgeId", "Badge id");
        award.Arguments.Add(youthId);
        award.Arguments.Add(badgeId);
        SetSenderAction(award, runtime, actor, result =>
            new AwardBadgeCommand(
                result.GetRequiredValue(badgeId),
                result.GetRequiredValue(youthId)));
        group.Subcommands.Add(award);
        return group;
    }

    private static Command BuildTokenCommands(CliRuntime? runtime, Option<Guid?> actor)
    {
        var group = new Command("token", "Issue short-lived API access tokens");
        var issue = new Command("issue", "Issue and audit a token for an active user");
        var user = RequiredOption<string>("--user", "User email");
        issue.Options.Add(user);
        SetAction(issue, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            var token = await services.GetRequiredService<ISender>().Send(
                new IssueOperationalTokenQuery(result.GetRequiredValue(user)),
                cancellationToken);
            await WriteJsonAsync(runtime!.Output, token);
        });
        group.Subcommands.Add(issue);
        return group;
    }

    private static void SetSenderAction<TRequest>(
        Command command,
        CliRuntime? runtime,
        Option<Guid?> actor,
        Func<ParseResult, TRequest> request)
        where TRequest : IRequest
    {
        SetAction(command, runtime, actor, requiresActor: true, async (result, services, cancellationToken) =>
        {
            await services.GetRequiredService<ISender>().Send(request(result), cancellationToken);
            await runtime!.Output.WriteLineAsync("Completed.");
        });
    }

    private static void SetAction(
        Command command,
        CliRuntime? runtime,
        Option<Guid?> actor,
        bool requiresActor,
        Func<ParseResult, IServiceProvider, CancellationToken, Task> action)
    {
        command.SetAction(async (result, cancellationToken) =>
        {
            if (runtime is null)
            {
                return 1;
            }

            try
            {
                await using var scope = runtime.Services.CreateAsyncScope();
                if (requiresActor)
                {
                    await SetActorAsync(
                        scope.ServiceProvider,
                        result.GetValue(actor),
                        cancellationToken);
                }

                await action(result, scope.ServiceProvider, cancellationToken);
                return 0;
            }
            catch (ValidationException exception)
            {
                var message = exception.Errors.Any()
                    ? string.Join(" ", exception.Errors.Select(error => error.ErrorMessage).Distinct())
                    : exception.Message;
                await runtime.Error.WriteLineAsync($"error: {message}");
                return 2;
            }
            catch (AppException exception)
            {
                await runtime.Error.WriteLineAsync($"error: {exception.Message}");
                return 2;
            }
            catch (OptionsValidationException exception)
            {
                await runtime.Error.WriteLineAsync($"error: {string.Join(" ", exception.Failures)}");
                return 2;
            }
            catch (IOException exception)
            {
                await runtime.Error.WriteLineAsync($"error: {exception.Message}");
                return 2;
            }
            catch (Exception exception)
            {
                await runtime.Error.WriteLineAsync(
                    $"error: operation failed ({exception.GetType().Name}).");
                return 1;
            }
        });
    }

    private static async Task SetActorAsync(
        IServiceProvider services,
        Guid? actorId,
        CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<IAppDbContext>();
        var query = db.Users.AsNoTracking().Where(user =>
            user.Role == Roles.ProgramAdministrator
            && user.Status == UserStatus.Active);
        var actor = actorId is Guid selectedId
            ? await query.FirstOrDefaultAsync(user => user.Id == selectedId, cancellationToken)
            : await query.OrderBy(user => user.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (actor is null)
        {
            throw new ForbiddenException(
                actorId.HasValue
                    ? "The selected actor is not an active Program Administrator."
                    : "No active Program Administrator is available; run 'wu db seed' first.");
        }

        services.GetRequiredService<OperationalCurrentUser>().Set(actor.Id, actor.Role);
    }

    private static Option<T> RequiredOption<T>(string name, string description) =>
        new(name)
        {
            Description = description,
            Required = true,
        };

    private static Argument<T> RequiredArgument<T>(string name, string description) =>
        new(name)
        {
            Description = description,
        };

    private static Task WriteJsonAsync<T>(TextWriter output, T value) =>
        output.WriteLineAsync(JsonSerializer.Serialize(value, JsonOptions));

    private sealed record CliRuntime(IServiceProvider Services, TextWriter Output, TextWriter Error);
}

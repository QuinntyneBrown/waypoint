using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Admin;

// Acceptance Test
// Traces to: L2-ADMIN-006 (organization settings)
[TestFixture]
[Property("Req", "L2-ADMIN-006")]
public sealed class OrgSettingsTests : IntegrationTestBase
{
    private async Task<(Guid Id, string Token)> SeedUserAsync(
        string role,
        Guid? guardianUserId = null)
    {
        var email = $"settings.{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        const string password = "Passw0rd!";
        Guid userId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = new User
            {
                Email = email,
                DisplayName = $"Settings {role}",
                Role = role,
                Status = UserStatus.Active,
                GuardianUserId = guardianUserId,
                PasswordHash = hasher.Hash(password),
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            userId = user.Id;
        }

        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<AuthResult>();
        return (userId, auth!.AccessToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body, string? token)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        if (token is not null)
        {
            Authorize(request, token);
        }
        return await Client.SendAsync(request);
    }

    [Test]
    [Property("Req", "L2-ADMIN-006")]
    public async Task Updating_org_settings_persists_and_non_admin_is_forbidden()
    {
        // Traces to: L2-ADMIN-006
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var body = new
        {
            name = "Word Up Community",
            displayName = "WUC",
            logoUrl = "https://example.com/logo.png",
            city = "Abuja",
            contactEmail = "hello@wordup.example",
            consentStatement = "We care about consent.",
            requireReConsentEachTerm = false,
            requireMediaConsent = true,
        };

        var update = await SendAsync(HttpMethod.Put, "/api/org-settings", body, adminToken);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var get = await SendAsync(HttpMethod.Get, "/api/org-settings", null, adminToken);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var settings = await get.Content.ReadFromJsonAsync<OrgSettingsDto>();
        Assert.That(settings!.Name, Is.EqualTo("Word Up Community"));
        Assert.That(settings.City, Is.EqualTo("Abuja"));
        Assert.That(settings.RequireMediaConsent, Is.True);
        Assert.That(settings.RequireReConsentEachTerm, Is.False);

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(await db.AuditEntries.AnyAsync(item => item.Action == "OrgSettingsUpdated"),
                Is.True);
        }

        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await SendAsync(HttpMethod.Put, "/api/org-settings", body, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Terms_can_be_created_and_advanced_through_validated_audited_statuses()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var start = DateTime.UtcNow.Date.AddDays(10);
        var create = await SendAsync(HttpMethod.Post, "/api/org-settings/terms", new
        {
            name = "Autumn 2026",
            startDate = start,
            endDate = start.AddMonths(3),
            status = "Upcoming",
        }, adminToken);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var created = await create.Content.ReadFromJsonAsync<TermResultDto>();

        using (var termScope = Factory.Services.CreateScope())
        {
            var termDb = termScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var otherActiveTerms = await termDb.Terms
                .Where(item => item.Id != created!.Id && item.Status == "Active")
                .ToListAsync();
            foreach (var term in otherActiveTerms)
            {
                term.Status = "Completed";
            }
            await termDb.SaveChangesAsync();
        }

        var activate = await SendAsync(HttpMethod.Put, $"/api/org-settings/terms/{created!.Id}", new
        {
            name = "Autumn 2026",
            startDate = DateTime.UtcNow.Date.AddDays(-1),
            endDate = DateTime.UtcNow.Date.AddMonths(3),
            status = "Active",
        }, adminToken);
        Assert.That(activate.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var list = await SendAsync(HttpMethod.Get, "/api/org-settings/terms", null, adminToken);
        var terms = await list.Content.ReadFromJsonAsync<List<TermDto>>();
        Assert.That(terms!.Single(item => item.Id == created.Id).Status, Is.EqualTo("Active"));

        var invalid = await SendAsync(HttpMethod.Post, "/api/org-settings/terms", new
        {
            name = "Invalid term",
            startDate = start,
            endDate = start.AddDays(-1),
            status = "Unknown",
        }, adminToken);
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await db.AuditEntries.AnyAsync(item => item.Action == "TermCreated"), Is.True);
        Assert.That(await db.AuditEntries.AnyAsync(item => item.Action == "TermUpdated"), Is.True);
    }

    [Test]
    public async Task Disabling_a_track_preserves_progress_but_pauses_new_enrolment_until_reenabled()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var existingYouth = await SeedUserAsync(Roles.Youth);
        var newYouth = await SeedUserAsync(Roles.Youth);
        Guid trackId;
        Guid moduleId;
        Guid cohortId;
        Guid progressId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var track = new Track
            {
                Name = "Paused track",
                Key = $"paused-{Guid.NewGuid():N}",
                Status = PublishStatus.Published,
                Enabled = true,
            };
            var module = new Module
            {
                TrackId = track.Id,
                Name = "Retained module",
                Status = PublishStatus.Published,
            };
            var lesson = new Lesson
            {
                ModuleId = module.Id,
                Title = "Retained lesson",
                Status = PublishStatus.Published,
            };
            var cohort = new Cohort { Name = "Paused cohort", TrackId = track.Id };
            var progress = new LessonProgress
            {
                YouthUserId = existingYouth.Id,
                LessonId = lesson.Id,
                Status = ProgressStatus.Completed,
            };
            db.AddRange(
                track,
                module,
                lesson,
                cohort,
                progress,
                new YouthTrackEnrollment
                {
                    YouthUserId = existingYouth.Id,
                    TrackId = track.Id,
                });
            await db.SaveChangesAsync();
            trackId = track.Id;
            moduleId = module.Id;
            cohortId = cohort.Id;
            progressId = progress.Id;
        }

        var disable = await SendAsync(
            HttpMethod.Post,
            $"/api/org-settings/tracks/{trackId}/enabled",
            new { enabled = false },
            adminToken);
        Assert.That(disable.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var pausedEnrollment = await SendAsync(
            HttpMethod.Post,
            $"/api/cohorts/{cohortId}/youth",
            new { youthId = newYouth.Id },
            adminToken);
        Assert.That(pausedEnrollment.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        var pausedWork = await SendAsync(HttpMethod.Post, "/api/mentor/assign-work", new
        {
            itemType = "Module",
            itemId = moduleId,
            youthIds = new[] { existingYouth.Id },
            cohortId = (Guid?)null,
            dueDate = (DateTime?)null,
            reminderCadence = (string?)null,
            message = (string?)null,
        }, adminToken);
        Assert.That(pausedWork.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(await db.LessonProgress.AnyAsync(item => item.Id == progressId), Is.True);
        }

        var enable = await SendAsync(
            HttpMethod.Post,
            $"/api/org-settings/tracks/{trackId}/enabled",
            new { enabled = true },
            adminToken);
        Assert.That(enable.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var resumedEnrollment = await SendAsync(
            HttpMethod.Post,
            $"/api/cohorts/{cohortId}/youth",
            new { youthId = newYouth.Id },
            adminToken);
        Assert.That(resumedEnrollment.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var resumedWork = await SendAsync(HttpMethod.Post, "/api/mentor/assign-work", new
        {
            itemType = "Module",
            itemId = moduleId,
            youthIds = new[] { existingYouth.Id },
            cohortId = (Guid?)null,
            dueDate = (DateTime?)null,
            reminderCadence = (string?)null,
            message = (string?)null,
        }, adminToken);
        Assert.That(resumedWork.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Reconsent_policy_requires_a_valid_consent_for_the_current_active_term()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var guardian = await SeedUserAsync(Roles.Guardian);
        var youth = await SeedUserAsync(Roles.Youth, guardian.Id);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var term = new Term
            {
                Name = "Current term",
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddMonths(3),
                Status = "Active",
            };
            var cohort = new Cohort
            {
                Name = "Consent cohort",
                TermStart = term.StartDate,
                TermEnd = term.EndDate,
            };
            db.AddRange(term, cohort);
            await db.SaveChangesAsync();
            var persistedYouth = await db.Users.SingleAsync(item => item.Id == youth.Id);
            persistedYouth.CohortId = cohort.Id;
            db.GuardianConsents.Add(new GuardianConsent
            {
                YouthUserId = youth.Id,
                GuardianUserId = guardian.Id,
                GuardianName = "Guardian",
                Relationship = "Parent",
                ParticipationConsent = true,
                Signature = "Guardian",
                ReferenceId = "GC-PRIOR",
                Term = "Prior term",
                SignedAt = DateTime.UtcNow.AddMonths(-1),
                ValidFrom = DateTime.UtcNow.AddMonths(-1),
                ValidUntil = DateTime.UtcNow.AddMonths(6),
            });
            db.OrgSettings.Add(new OrgSettings
            {
                Name = "Word Up",
                RequireReConsentEachTerm = true,
            });
            await db.SaveChangesAsync();
        }

        var gated = await SendAsync(HttpMethod.Get, "/api/me/access", null, youth.Token);
        var gatedAccess = await gated.Content.ReadFromJsonAsync<AccessDto>();
        Assert.That(gatedAccess!.ConsentRequired, Is.True);

        var settings = new
        {
            name = "Word Up",
            displayName = "Word Up",
            logoUrl = (string?)null,
            city = "Toronto",
            contactEmail = "consent@wordup.example",
            consentStatement = "Current participation policy",
            requireReConsentEachTerm = false,
            requireMediaConsent = true,
        };
        var update = await SendAsync(HttpMethod.Put, "/api/org-settings", settings, adminToken);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var allowed = await SendAsync(HttpMethod.Get, "/api/me/access", null, youth.Token);
        var allowedAccess = await allowed.Content.ReadFromJsonAsync<AccessDto>();
        Assert.That(allowedAccess!.ConsentRequired, Is.False);
        Assert.That(allowedAccess.ParticipationAllowed, Is.True);
    }

    private sealed record OrgSettingsDto(
        string Name,
        string? DisplayName,
        string? LogoUrl,
        string? City,
        string? ContactEmail,
        string? ConsentStatement,
        bool RequireReConsentEachTerm,
        bool RequireMediaConsent);
    private sealed record TermResultDto(Guid Id);
    private sealed record TermDto(
        Guid Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        string Status);
    private sealed record AccessDto(bool HasCohort, bool ConsentRequired, bool ParticipationAllowed);
}

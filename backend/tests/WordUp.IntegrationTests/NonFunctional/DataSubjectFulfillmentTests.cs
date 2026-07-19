using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

// Acceptance Tests
// Traces to: L2-NFR-003 AC1-AC3; L2-CONSENT-001
[TestFixture]
[Property("Req", "L2-NFR-003")]
public sealed class DataSubjectFulfillmentTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        string token,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        Authorize(request, token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return await Client.SendAsync(request);
    }

    private async Task<(User User, string Token)> SeedAndSignInAsync(
        string role,
        string? email = null)
    {
        var user = new User
        {
            Email = email ?? $"privacy.{Guid.NewGuid():N}@example.com",
            DisplayName = $"Privacy {role}",
            Pronouns = "private pronouns",
            Role = role,
            Status = UserStatus.Active,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            user.PasswordHash = hasher.Hash("Passw0rd!");
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var response = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { email = user.Email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthDto>();
        return (user, auth!.AccessToken);
    }

    private async Task<DataRequestDto> RequestAsync(string token, string actionType, string details)
    {
        var response = await SendAsync(
            HttpMethod.Post,
            "/api/me/data-requests",
            token,
            new { actionType, details });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DataRequestDto>())!;
    }

    [Test]
    public async Task Administrator_can_discover_and_fulfil_a_correction_request_once()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (admin, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var requested = await RequestAsync(subjectToken, "correction", "Correct my profile fields.");

        var forbidden = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            subjectToken,
            new { displayName = "Not allowed" });
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var list = await SendAsync(HttpMethod.Get, "/api/admin/data-requests?status=pending", adminToken);
        var page = await list.Content.ReadFromJsonAsync<DataRequestPageDto>();
        Assert.That(page!.Items, Has.Some.Matches<DataRequestDto>(item =>
            item.Id == requested.Id
            && item.SubjectUserId == subject.Id
            && item.Status == "Pending"
            && item.Details == "Correct my profile fields."));

        var correctedEmail = $"corrected.{Guid.NewGuid():N}@example.com";
        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            adminToken,
            new
            {
                displayName = "Corrected Participant",
                email = correctedEmail,
                pronouns = "they/them",
                resolutionNote = "Identity verified.",
            });
        Assert.That(fulfilled.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var duplicate = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            adminToken,
            new { displayName = "Second change" });
        Assert.That(duplicate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var changed = await db.Users.FindAsync(subject.Id);
        Assert.Multiple(() =>
        {
            Assert.That(changed!.DisplayName, Is.EqualTo("Corrected Participant"));
            Assert.That(changed.Email, Is.EqualTo(correctedEmail));
            Assert.That(changed.Pronouns, Is.EqualTo("they/them"));
            Assert.That(db.AuditEntries.Any(entry =>
                entry.Action == "DataSubjectRequestFulfilled:correction"
                && entry.ActorUserId == admin.Id
                && entry.TargetId == requested.Id), Is.True);
        });
    }

    [Test]
    public async Task Removal_anonymizes_personal_content_and_sessions_but_preserves_aggregate_rows()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (admin, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var attendanceId = Guid.NewGuid();
        var progressId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AttendanceRecords.Add(new AttendanceRecord
            {
                Id = attendanceId,
                SessionId = Guid.NewGuid(),
                YouthUserId = subject.Id,
                Status = AttendanceStatus.Present,
                Note = "private attendance note",
                RecordedByUserId = admin.Id,
            });
            db.LessonProgress.Add(new LessonProgress
            {
                Id = progressId,
                YouthUserId = subject.Id,
                LessonId = Guid.NewGuid(),
                Status = ProgressStatus.Completed,
                CompletedAt = DateTime.UtcNow,
            });
            db.CheckIns.Add(new CheckIn
            {
                YouthUserId = subject.Id,
                MentorUserId = admin.Id,
                Type = "1:1",
                GoalsDiscussed = "private goal",
                PrayerRequests = "private prayer",
                ActionItems = "private action",
            });
            db.Messages.Add(new Message
            {
                ThreadKey = Message.KeyFor(subject.Id, admin.Id),
                SenderId = subject.Id,
                RecipientId = admin.Id,
                Body = "private message",
            });
            db.StoredUserFiles.Add(new StoredUserFile
            {
                Id = fileId,
                OwnerUserId = subject.Id,
                Purpose = StoredUserFilePurpose.ProfilePhoto,
                OriginalFileName = "private.png",
                ContentType = "image/png",
                SizeBytes = 4,
                Sha256 = new string('A', 64),
                Content = [1, 2, 3, 4],
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        var requested = await RequestAsync(subjectToken, "removal", "Remove my personal data.");
        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            adminToken,
            new { resolutionNote = "Removal approved." });
        Assert.That(fulfilled.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var staleSession = await SendAsync(HttpMethod.Get, "/api/me/data", subjectToken);
        Assert.That(staleSession.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        using var verifyScope = Factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var removed = await verify.Users.FindAsync(subject.Id);
        var checkIn = await verify.CheckIns.SingleAsync(item => item.YouthUserId == subject.Id);
        var message = await verify.Messages.SingleAsync(item => item.SenderId == subject.Id);
        Assert.Multiple(() =>
        {
            Assert.That(removed!.Status, Is.EqualTo(UserStatus.Removed));
            Assert.That(removed.DisplayName, Is.EqualTo("Former participant"));
            Assert.That(removed.Email, Does.EndWith("@anonymized.invalid"));
            Assert.That(removed.Pronouns, Is.Null);
            Assert.That(removed.PersonalDataAnonymizedAt, Is.Not.Null);
            Assert.That(verify.RefreshTokens.Any(item => item.UserId == subject.Id && item.RevokedAt == null), Is.False);
            Assert.That(verify.StoredUserFiles.Any(item => item.Id == fileId), Is.False);
            Assert.That(checkIn.GoalsDiscussed, Is.Null);
            Assert.That(checkIn.PrayerRequests, Is.Null);
            Assert.That(checkIn.ActionItems, Is.Null);
            Assert.That(message.Body, Is.EqualTo("[removed]"));
            Assert.That(verify.AttendanceRecords.Any(item => item.Id == attendanceId), Is.True);
            Assert.That(verify.LessonProgress.Any(item => item.Id == progressId), Is.True);
        });
    }

    [Test]
    public async Task Consent_withdrawal_fulfilment_withdraws_active_subject_consent()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var consent = new GuardianConsent
        {
            YouthUserId = subject.Id,
            GuardianName = "Guardian",
            Relationship = "Parent",
            ParticipationConsent = true,
            Signature = "Guardian",
            Method = "typed",
            ReferenceId = "CONSENT-PRIVACY",
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidUntil = DateTime.UtcNow.AddYears(1),
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GuardianConsents.Add(consent);
            await db.SaveChangesAsync();
        }

        var requested = await RequestAsync(subjectToken, "consent-withdrawal", "Withdraw consent.");
        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            adminToken,
            new { resolutionNote = "Withdrawal completed." });
        Assert.That(fulfilled.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = Factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That((await verify.GuardianConsents.FindAsync(consent.Id))!.WithdrawnAt, Is.Not.Null);
    }

    [Test]
    public async Task Retention_run_anonymizes_only_expired_deactivated_users_and_keeps_aggregates()
    {
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var retentionCohortId = Guid.NewGuid();
        var old = new User
        {
            Email = $"old.{Guid.NewGuid():N}@example.com",
            DisplayName = "Expired Person",
            PasswordHash = "unused",
            Role = Roles.Youth,
            Status = UserStatus.Deactivated,
            DeactivatedAt = DateTime.UtcNow.AddDays(-400),
            CohortId = retentionCohortId,
        };
        var recent = new User
        {
            Email = $"recent.{Guid.NewGuid():N}@example.com",
            DisplayName = "Recent Person",
            PasswordHash = "unused",
            Role = Roles.Youth,
            Status = UserStatus.Deactivated,
            DeactivatedAt = DateTime.UtcNow.AddDays(-30),
            CohortId = retentionCohortId,
        };
        var active = new User
        {
            Email = $"active.{Guid.NewGuid():N}@example.com",
            DisplayName = "Active Person",
            PasswordHash = "unused",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow.AddYears(-3),
            CohortId = retentionCohortId,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.AddRange(old, recent, active);
            db.AttendanceRecords.AddRange(
                AttendanceFor(old.Id),
                AttendanceFor(recent.Id));
            await db.SaveChangesAsync();
        }

        var response = await SendAsync(
            HttpMethod.Post,
            "/api/admin/privacy/retention",
            adminToken,
            new { });
        var result = await response.Content.ReadFromJsonAsync<RetentionResultDto>();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.AnonymizedCount, Is.EqualTo(1));
        });

        var reportResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/hub?cohortId={retentionCohortId}",
            adminToken);
        var report = await reportResponse.Content.ReadFromJsonAsync<ReportAggregateDto>();
        Assert.Multiple(() =>
        {
            Assert.That(report!.RecordCount, Is.EqualTo(3),
                "the anonymized youth remains in de-identified population aggregates");
            Assert.That(report.Attendance.Total, Is.EqualTo(2),
                "anonymization preserves de-identified attendance aggregates");
        });

        using var verifyScope = Factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That((verify.Users.Find(old.Id)!).Status, Is.EqualTo(UserStatus.Removed));
            Assert.That((verify.Users.Find(recent.Id)!).Status, Is.EqualTo(UserStatus.Deactivated));
            Assert.That((verify.Users.Find(active.Id)!).Status, Is.EqualTo(UserStatus.Active));
            Assert.That(verify.AttendanceRecords.Count(item =>
                item.YouthUserId == old.Id || item.YouthUserId == recent.Id), Is.EqualTo(2));
            Assert.That(verify.AuditEntries.Any(item => item.Action == "RetentionAnonymizationRun"), Is.True);
        });
    }

    [Test]
    public void Sensitive_free_text_uses_an_authenticated_at_rest_converter()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var protector = scope.ServiceProvider.GetRequiredService<ISensitiveDataProtector>();
        const string secret = "private safeguarding detail";

        var protectedValue = protector.Protect(secret);
        var tampered = protectedValue.ToCharArray();
        tampered["enc:v1:".Length + 4] = tampered["enc:v1:".Length + 4] == 'A' ? 'B' : 'A';
        var checkInConverter = db.Model.FindEntityType(typeof(CheckIn))!
            .FindProperty(nameof(CheckIn.PrayerRequests))!
            .GetValueConverter();
        var requestConverter = db.Model.FindEntityType(typeof(DataSubjectRequest))!
            .FindProperty(nameof(DataSubjectRequest.Details))!
            .GetValueConverter();
        var otherSensitiveConverters = new[]
        {
            (typeof(ReflectionEntry), nameof(ReflectionEntry.Text)),
            (typeof(Submission), nameof(Submission.BodyText)),
            (typeof(AttendanceRecord), nameof(AttendanceRecord.Note)),
            (typeof(SupportRequest), nameof(SupportRequest.Description)),
            (typeof(GuardianProfile), nameof(GuardianProfile.Phone)),
            (typeof(GuardianConsent), nameof(GuardianConsent.Signature)),
        }.Select(property => db.Model.FindEntityType(property.Item1)!
            .FindProperty(property.Item2)!
            .GetValueConverter())
        .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(protectedValue, Does.Not.Contain(secret));
            Assert.That(protector.Unprotect(protectedValue), Is.EqualTo(secret));
            Assert.Throws<AuthenticationTagMismatchException>(() =>
                protector.Unprotect(new string(tampered)));
            Assert.That(checkInConverter, Is.Not.Null);
            Assert.That(requestConverter, Is.Not.Null);
            Assert.That(checkInConverter!.ConvertToProvider(secret), Is.Not.EqualTo(secret));
            Assert.That(requestConverter!.ConvertToProvider(secret), Is.Not.EqualTo(secret));
            Assert.That(otherSensitiveConverters, Has.All.Not.Null);
            Assert.That(otherSensitiveConverters.Select(converter => converter!.ConvertToProvider(secret)),
                Has.All.Not.EqualTo(secret));
        });
    }

    private static AttendanceRecord AttendanceFor(Guid youthId) => new()
    {
        SessionId = Guid.NewGuid(),
        YouthUserId = youthId,
        Status = AttendanceStatus.Present,
        RecordedByUserId = Guid.NewGuid(),
        RecordedAt = DateTime.UtcNow.AddDays(-10),
    };

    private sealed record AuthDto(string AccessToken);
    private sealed record DataRequestDto(
        Guid Id,
        string Reference,
        Guid SubjectUserId,
        string ActionType,
        string Status,
        DateTime RequestedAt,
        string? Details);
    private sealed record DataRequestPageDto(IReadOnlyList<DataRequestDto> Items, int Total);
    private sealed record RetentionResultDto(int AnonymizedCount, DateTime Cutoff);
    private sealed record ReportAggregateDto(int RecordCount, AttendanceAggregateDto Attendance);
    private sealed record AttendanceAggregateDto(int Total);
}

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.NonFunctional;

// Acceptance Tests
// Traces to: L2-NFR-003 AC2
[TestFixture]
[Property("Req", "L2-NFR-003")]
public sealed class PersonalDataExportTests : IntegrationTestBase
{
    [Test]
    public async Task Export_requires_authentication_and_a_completed_access_request()
    {
        var anonymous = await Client.GetAsync("/api/me/data/export");
        Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var requested = await PostAuthorizedAsync(
            "/api/me/data-requests",
            subjectToken,
            new { actionType = "access", details = "Provide my complete record." });
        requested.EnsureSuccessStatusCode();
        var accessRequest = await requested.Content.ReadFromJsonAsync<DataRequestDto>();

        var pendingExport = await GetAuthorizedAsync("/api/me/data/export", subjectToken);
        Assert.That(pendingExport.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var fulfilled = await PostAuthorizedAsync(
            $"/api/admin/data-requests/{accessRequest!.Id}/fulfill",
            adminToken,
            new { resolutionNote = "Identity verified." });
        fulfilled.EnsureSuccessStatusCode();

        var export = await GetAuthorizedAsync("/api/me/data/export", subjectToken);
        Assert.Multiple(() =>
        {
            Assert.That(export.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(export.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            Assert.That(export.Content.Headers.ContentDisposition?.DispositionType, Is.EqualTo("attachment"));
            Assert.That(export.Content.Headers.ContentDisposition?.FileNameStar, Does.Contain(subject.Id.ToString("N")));
            Assert.That(export.Headers.CacheControl?.NoStore, Is.True);
        });
    }

    [Test]
    public async Task Completed_export_contains_subject_records_and_owned_files_but_not_secrets_or_third_party_identity()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (staff, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var now = DateTime.UtcNow;
        var lessonId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var submission = new Submission
        {
            YouthUserId = subject.Id,
            AssignmentId = assignmentId,
            BodyText = "subject submission text",
            YouthNote = "subject submission note",
            MentorComment = "feedback about the subject",
            ReviewedByUserId = staff.Id,
            ReviewedAt = now,
        };
        var attendance = new AttendanceRecord
        {
            SessionId = sessionId,
            YouthUserId = subject.Id,
            Status = AttendanceStatus.Present,
            Note = "subject attendance note",
            RecordedByUserId = staff.Id,
            RecordedAt = now,
        };

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var trackedSubject = await db.Users.FindAsync(subject.Id);
            trackedSubject!.PasswordHash = "PASSWORD_HASH_SENTINEL";
            trackedSubject.Pronouns = "subject pronouns";

            db.RefreshTokens.Add(new RefreshToken
            {
                UserId = subject.Id,
                TokenHash = "REFRESH_TOKEN_HASH_SENTINEL",
                ExpiresAt = now.AddDays(1),
            });
            db.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = subject.Id,
                TokenHash = "RESET_TOKEN_HASH_SENTINEL",
                ExpiresAt = now.AddHours(1),
            });
            db.Applications.Add(new YouthApplication
            {
                FirstName = "Subject",
                LastName = "Applicant",
                Age = 15,
                WhoIsFilling = "self",
                Neighbourhood = "Subject neighbourhood",
                CohortInterest = "Subject cohort interest",
                WhyInterested = "Subject application narrative",
                GuardianName = "THIRD_PARTY_GUARDIAN_NAME",
                GuardianEmail = "third.party.guardian@example.com",
                GuardianPhone = "THIRD_PARTY_GUARDIAN_PHONE",
                ParticipationConsentAck = true,
                PrivacyAck = true,
                CreatedYouthUserId = subject.Id,
            });
            db.GuardianConsents.Add(new GuardianConsent
            {
                YouthUserId = subject.Id,
                GuardianUserId = staff.Id,
                GuardianName = "THIRD_PARTY_CONSENT_NAME",
                Relationship = "THIRD_PARTY_RELATIONSHIP",
                ParticipationConsent = true,
                Signature = "THIRD_PARTY_SIGNATURE",
                ReferenceId = "CONSENT-REFERENCE",
                ValidFrom = now.AddDays(-1),
                ValidUntil = now.AddYears(1),
            });
            db.GuardianConsentRequests.Add(new GuardianConsentRequest
            {
                YouthUserId = subject.Id,
                GuardianUserId = staff.Id,
                Destination = "THIRD_PARTY_CONSENT_DESTINATION",
                TokenHash = "CONSENT_TOKEN_HASH_SENTINEL",
                ExpiresAt = now.AddDays(1),
            });
            db.AttendanceRecords.Add(attendance);
            db.LessonProgress.Add(new LessonProgress
            {
                YouthUserId = subject.Id,
                LessonId = lessonId,
                Status = ProgressStatus.InProgress,
            });
            db.CodingLessonWorkspaces.Add(new CodingLessonWorkspace
            {
                YouthUserId = subject.Id,
                LessonId = lessonId,
                Code = "console.log('subject workspace');",
            });
            db.Submissions.Add(submission);
            db.StoredSubmissionFiles.Add(new StoredSubmissionFile
            {
                OwnerUserId = subject.Id,
                AssignmentId = assignmentId,
                Kind = SubmissionFileKind.File,
                OriginalFileName = "subject-evidence.txt",
                ContentType = "text/plain",
                SizeBytes = 18,
                Sha256 = new string('a', 64),
                Content = Encoding.UTF8.GetBytes("owned file content"),
            });
            db.SubmissionTimelineEvents.Add(new SubmissionTimelineEvent
            {
                SubmissionId = submission.Id,
                Type = "submitted",
            });
            db.Reflections.Add(new ReflectionEntry
            {
                YouthUserId = subject.Id,
                LessonId = lessonId,
                Text = "subject reflection",
                Mood = "hopeful",
            });
            db.CheckIns.Add(new CheckIn
            {
                YouthUserId = subject.Id,
                MentorUserId = staff.Id,
                Date = now,
                Type = "1:1",
                GoalsDiscussed = "subject goal",
                PrayerRequests = "PRIVATE_STAFF_PRAYER_NOTE",
                ActionItems = "subject action item",
                Visibility = Visibility.Private,
            });
            db.Messages.Add(new Message
            {
                ThreadKey = Message.KeyFor(subject.Id, staff.Id),
                SenderId = staff.Id,
                RecipientId = subject.Id,
                Body = "message received by subject",
            });
            db.Notifications.Add(new Notification
            {
                UserId = subject.Id,
                EventKey = "privacy-test",
                Title = "Subject notification",
                Body = "notification body for subject",
            });
            db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = subject.Id,
                EventKey = "privacy-test",
                InApp = true,
                Email = true,
            });
            db.DisplayPreferences.Add(new DisplayPreference
            {
                UserId = subject.Id,
                TextScalePercent = 125,
                HighContrast = true,
            });
            db.SupportRequests.Add(new SupportRequest
            {
                UserId = subject.Id,
                Category = "account",
                Description = "subject support description",
                ReplyToEmail = subject.Email,
            });
            db.StoredUserFiles.Add(new StoredUserFile
            {
                OwnerUserId = subject.Id,
                Purpose = StoredUserFilePurpose.SupportScreenshot,
                OriginalFileName = "support.png",
                ContentType = "image/png",
                SizeBytes = 4,
                Sha256 = new string('b', 64),
                Content = [1, 2, 3, 4],
                CreatedAt = now,
            });
            db.ContactRequests.AddRange(
                new ContactRequest
                {
                    OwnerUserId = subject.Id,
                    Name = subject.DisplayName,
                    Email = subject.Email.ToUpperInvariant(),
                    Topic = "subject topic",
                    Message = "subject public contact message",
                },
                new ContactRequest
                {
                    Name = "Other Person",
                    Email = subject.Email,
                    Topic = "other topic",
                    Message = "UNRELATED_PUBLIC_MESSAGE",
                });
            db.AuditEntries.Add(new AuditEntry
            {
                ActorUserId = staff.Id,
                Action = "AttendanceUpdated",
                TargetType = "AttendanceRecord",
                TargetId = attendance.Id,
                Details = "THIRD_PARTY_AUDIT_DETAILS",
                Source = "THIRD_PARTY_AUDIT_SOURCE",
            });
            await db.SaveChangesAsync();
        }

        var requested = await PostAuthorizedAsync(
            "/api/me/data-requests",
            subjectToken,
            new { actionType = "access", details = "Complete export." });
        requested.EnsureSuccessStatusCode();
        var accessRequest = await requested.Content.ReadFromJsonAsync<DataRequestDto>();
        var fulfilled = await PostAuthorizedAsync(
            $"/api/admin/data-requests/{accessRequest!.Id}/fulfill",
            adminToken,
            new { resolutionNote = "Completed for export test." });
        fulfilled.EnsureSuccessStatusCode();

        var response = await GetAuthorizedAsync("/api/me/data/export", subjectToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Multiple(() =>
        {
            Assert.That(root.GetProperty("account").GetProperty("id").GetGuid(), Is.EqualTo(subject.Id));
            Assert.That(root.GetProperty("applicationAndConsent").GetProperty("applications").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("attendanceAndEngagement").GetProperty("attendance").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("learning").GetProperty("codingWorkspaces").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("learning").GetProperty("submissions").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("learning").GetProperty("submissionFiles")[0]
                .GetProperty("contentBase64").GetString(),
                Is.EqualTo(Convert.ToBase64String(Encoding.UTF8.GetBytes("owned file content"))));
            Assert.That(root.GetProperty("mentoring").GetProperty("checkIns").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("communicationsAndPreferences").GetProperty("messages").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("communicationsAndPreferences").GetProperty("supportRequests").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("privacyAndAudit").GetProperty("dataSubjectRequests").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("accountLinkedPublicSubmissions").GetProperty("contactRequests").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("publicSubmissionScope").GetProperty("unlinkedHandling").GetString(),
                Does.Contain("record IDs"));
        });

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("Subject application narrative"));
            Assert.That(json, Does.Contain("subject attendance note"));
            Assert.That(json, Does.Contain("subject workspace"));
            Assert.That(json, Does.Contain("subject public contact message"));
            Assert.That(json, Does.Not.Contain("PASSWORD_HASH_SENTINEL"));
            Assert.That(json, Does.Not.Contain("REFRESH_TOKEN_HASH_SENTINEL"));
            Assert.That(json, Does.Not.Contain("RESET_TOKEN_HASH_SENTINEL"));
            Assert.That(json, Does.Not.Contain("CONSENT_TOKEN_HASH_SENTINEL"));
            Assert.That(json, Does.Not.Contain("THIRD_PARTY_GUARDIAN_NAME"));
            Assert.That(json, Does.Not.Contain("THIRD_PARTY_CONSENT_NAME"));
            Assert.That(json, Does.Not.Contain("THIRD_PARTY_SIGNATURE"));
            Assert.That(json, Does.Not.Contain("THIRD_PARTY_CONSENT_DESTINATION"));
            Assert.That(json, Does.Not.Contain("THIRD_PARTY_AUDIT_DETAILS"));
            Assert.That(json, Does.Not.Contain("THIRD_PARTY_AUDIT_SOURCE"));
            Assert.That(json, Does.Not.Contain("PRIVATE_STAFF_PRAYER_NOTE"));
            Assert.That(json, Does.Not.Contain("UNRELATED_PUBLIC_MESSAGE"));
            Assert.That(json, Does.Not.Contain(staff.Email));
            Assert.That(json, Does.Not.Contain(staff.Id.ToString()));
        });

        using var verificationScope = Factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(verificationDb.AuditEntries.Any(item =>
            item.Action == "PersonalDataExportDownloaded"
            && item.ActorUserId == subject.Id
            && item.TargetId == subject.Id), Is.True);
    }

    [Test]
    public async Task Request_history_pages_beyond_the_first_twenty_five_without_truncation()
    {
        var (subject, token) = await SeedAndSignInAsync(Roles.Youth);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.DataSubjectRequests.AddRange(Enumerable.Range(1, 31).Select(index =>
                new DataSubjectRequest
                {
                    Reference = $"DSR-{index:000}-{Guid.NewGuid():N}"[..16],
                    SubjectUserId = subject.Id,
                    ActionType = "access",
                    RequestedAt = DateTime.UtcNow.AddMinutes(-index),
                }));
            await db.SaveChangesAsync();
        }

        var first = await GetAuthorizedAsync("/api/me/data-requests?page=1&pageSize=25", token);
        var second = await GetAuthorizedAsync("/api/me/data-requests?page=2&pageSize=25", token);
        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();
        var firstPage = await first.Content.ReadFromJsonAsync<RequestPageDto>();
        var secondPage = await second.Content.ReadFromJsonAsync<RequestPageDto>();

        Assert.Multiple(() =>
        {
            Assert.That(firstPage!.Total, Is.EqualTo(31));
            Assert.That(firstPage.Items, Has.Count.EqualTo(25));
            Assert.That(secondPage!.Total, Is.EqualTo(31));
            Assert.That(secondPage.Items, Has.Count.EqualTo(6));
            Assert.That(firstPage.Items.Select(item => item.Id)
                .Intersect(secondPage.Items.Select(item => item.Id)), Is.Empty);
        });
    }

    [Test]
    public async Task Guardian_contact_export_includes_only_the_guardians_own_application_fields()
    {
        var (guardian, guardianToken) = await SeedAndSignInAsync(Roles.Guardian);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var youthCohortId = Guid.NewGuid();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Applications.Add(new YouthApplication
            {
                FirstName = "YOUTH_FIRST_SENTINEL",
                LastName = "YOUTH_LAST_SENTINEL",
                Age = 14,
                WhoIsFilling = "guardian",
                Neighbourhood = "YOUTH_NEIGHBOURHOOD_SENTINEL",
                CohortInterest = "YOUTH_COHORT_INTEREST_SENTINEL",
                WhyInterested = "YOUTH_NARRATIVE_SENTINEL",
                TrackInterest = "YOUTH_TRACK_SENTINEL",
                GuardianName = guardian.DisplayName,
                GuardianEmail = guardian.Email.ToUpperInvariant(),
                GuardianPhone = "GUARDIAN_PHONE_SENTINEL",
                GuardianUserId = guardian.Id,
                ParticipationConsentAck = true,
                PrivacyAck = true,
                Status = ApplicationStatus.Approved,
                DecisionNote = "YOUTH_DECISION_SENTINEL",
                CohortId = youthCohortId,
                ReviewedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        var requested = await PostAuthorizedAsync(
            "/api/me/data-requests",
            guardianToken,
            new { actionType = "access", details = "Guardian-contact export." });
        requested.EnsureSuccessStatusCode();
        var accessRequest = await requested.Content.ReadFromJsonAsync<DataRequestDto>();
        var fulfilled = await PostAuthorizedAsync(
            $"/api/admin/data-requests/{accessRequest!.Id}/fulfill",
            adminToken,
            new { resolutionNote = "Identity verified." });
        fulfilled.EnsureSuccessStatusCode();

        var response = await GetAuthorizedAsync("/api/me/data/export", guardianToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var application = document.RootElement
            .GetProperty("applicationAndConsent")
            .GetProperty("applications")[0];

        Assert.Multiple(() =>
        {
            Assert.That(application.GetProperty("perspective").GetString(), Is.EqualTo("guardian-contact"));
            Assert.That(application.GetProperty("guardianName").GetString(), Is.EqualTo(guardian.DisplayName));
            Assert.That(application.GetProperty("guardianEmail").GetString(), Is.EqualTo(guardian.Email.ToUpperInvariant()));
            Assert.That(application.GetProperty("guardianPhone").GetString(), Is.EqualTo("GUARDIAN_PHONE_SENTINEL"));
            foreach (var propertyName in new[]
            {
                "firstName", "lastName", "age", "whoIsFilling", "neighbourhood",
                "cohortInterest", "whyInterested", "trackInterest", "participationConsentAck",
                "privacyAck", "status", "decisionNote", "cohortId", "reviewedAt",
            })
            {
                Assert.That(application.GetProperty(propertyName).ValueKind, Is.EqualTo(JsonValueKind.Null), propertyName);
            }
        });
        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Not.Contain("YOUTH_FIRST_SENTINEL"));
            Assert.That(json, Does.Not.Contain("YOUTH_NARRATIVE_SENTINEL"));
            Assert.That(json, Does.Not.Contain("YOUTH_DECISION_SENTINEL"));
            Assert.That(json, Does.Not.Contain(youthCohortId.ToString()));
        });
    }

    private async Task<(User User, string Token)> SeedAndSignInAsync(string role)
    {
        var password = "Passw0rd!";
        var user = new User
        {
            Email = $"privacy-export.{Guid.NewGuid():N}@example.com",
            DisplayName = $"Privacy Export {role}",
            Role = role,
            Status = UserStatus.Active,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            user.PasswordHash = hasher.Hash(password);
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var response = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { user.Email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthDto>();
        return (user, auth!.AccessToken);
    }

    private async Task<HttpResponseMessage> GetAuthorizedAsync(string uri, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PostAuthorizedAsync(string uri, string token, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(body),
        };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private sealed record AuthDto(string AccessToken);
    private sealed record DataRequestDto(Guid Id);
    private sealed record RequestPageDto(IReadOnlyList<DataRequestDto> Items, int Total);
}

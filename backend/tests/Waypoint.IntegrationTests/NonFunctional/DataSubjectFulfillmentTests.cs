using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.NonFunctional;

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
    public async Task Correction_purges_pending_subject_snapshots_but_keeps_history_and_recipient_only_mail()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (recipient, _) = await SeedAndSignInAsync(Roles.Guardian);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var requested = await RequestAsync(subjectToken, "correction", "Correct my profile snapshot.");
        var pendingSubject = new OutboundEmail
        {
            ToEmail = recipient.Email,
            Subject = "Pending cross-recipient update",
            Body = $"Old profile name: {subject.DisplayName}",
            RecipientUserId = recipient.Id,
            SubjectUserId = subject.Id,
        };
        var sentSubject = new OutboundEmail
        {
            ToEmail = recipient.Email,
            Subject = "Historical cross-recipient update",
            Body = $"Historical profile name: {subject.DisplayName}",
            RecipientUserId = recipient.Id,
            SubjectUserId = subject.Id,
            SentAt = DateTime.UtcNow.AddMinutes(-5),
        };
        var recipientOnly = new OutboundEmail
        {
            ToEmail = subject.Email,
            Subject = "Account-only update",
            Body = "This body contains no profile snapshot.",
            RecipientUserId = subject.Id,
        };
        using (var seedScope = Factory.Services.CreateScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            seedDb.OutboundEmails.AddRange(pendingSubject, sentSubject, recipientOnly);
            await seedDb.SaveChangesAsync();
        }
        var correctedEmail = $"corrected-snapshot-{Guid.NewGuid():N}@example.com";

        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            adminToken,
            new
            {
                displayName = "Current Participant",
                email = correctedEmail,
            });
        fulfilled.EnsureSuccessStatusCode();

        using var verifyScope = Factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var completion = Factory.Emails.LastTo(correctedEmail);
        Assert.Multiple(() =>
        {
            Assert.That(verify.OutboundEmails.Any(item => item.Id == pendingSubject.Id), Is.False);
            Assert.That(verify.OutboundEmails.Any(item => item.Id == sentSubject.Id), Is.True);
            Assert.That(
                verify.OutboundEmails.Single(item => item.Id == recipientOnly.Id).ToEmail,
                Is.EqualTo(correctedEmail));
            Assert.That(completion, Is.Not.Null);
            Assert.That(completion!.RecipientUserId, Is.EqualTo(subject.Id));
            Assert.That(completion.SubjectUserId, Is.EqualTo(subject.Id));
            Assert.That(completion.Body, Does.Not.Contain(subject.DisplayName));
        });
    }

    [Test]
    public async Task Typed_correction_can_explicitly_clear_pronouns()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var create = await SendAsync(
            HttpMethod.Post,
            "/api/me/data-requests",
            subjectToken,
            new
            {
                actionType = "correction",
                correction = new { clearPronouns = true },
            });
        create.EnsureSuccessStatusCode();
        var requested = await create.Content.ReadFromJsonAsync<DataRequestDto>();

        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested!.Id}/fulfill",
            adminToken,
            new { clearPronouns = true });
        Assert.That(fulfilled.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var changed = await db.Users.FindAsync(subject.Id);
        Assert.That(changed!.Pronouns, Is.Null);
    }

    [Test]
    public async Task Administrator_can_attach_separately_verified_anonymous_records_during_fulfillment()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Guardian);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var contact = new ContactRequest
        {
            Name = subject.DisplayName,
            Email = subject.Email,
            Topic = "Verified ownership topic",
            Message = "VERIFIED_CONTACT_SENTINEL",
        };
        var application = new YouthApplication
        {
            FirstName = "Verified",
            LastName = "Applicant",
            Age = 14,
            WhoIsFilling = "guardian",
            Neighbourhood = "Verified neighbourhood",
            CohortInterest = "Verified cohort",
            WhyInterested = "VERIFIED_APPLICATION_SENTINEL",
            GuardianName = subject.DisplayName,
            GuardianEmail = subject.Email,
            GuardianPhone = "+1 416 555 0188",
            ParticipationConsentAck = true,
            PrivacyAck = true,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AddRange(contact, application);
            await db.SaveChangesAsync();
        }

        var access = await RequestAsync(subjectToken, "access", "Verify legacy records.");
        var unattested = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{access.Id}/fulfill",
            adminToken,
            new
            {
                verifiedContactRequestIds = new[] { contact.Id },
                verifiedGuardianApplicationIds = new[] { application.Id },
            });
        Assert.That(unattested.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{access.Id}/fulfill",
            adminToken,
            new
            {
                verifiedOwnershipAttested = true,
                verifiedContactRequestIds = new[] { contact.Id },
                verifiedGuardianApplicationIds = new[] { application.Id },
                resolutionNote = "Identity and record references verified out of band.",
            });
        fulfilled.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var fulfillmentAudit = await db.AuditEntries
                .Where(item => item.Action == "DataSubjectRequestFulfilled:access"
                    && item.TargetId == access.Id)
                .Select(item => item.Details)
                .ToListAsync();
            Assert.Multiple(() =>
            {
                Assert.That((db.ContactRequests.Find(contact.Id))!.OwnerUserId, Is.EqualTo(subject.Id));
                Assert.That((db.Applications.Find(application.Id))!.GuardianUserId, Is.EqualTo(subject.Id));
                Assert.That(fulfillmentAudit.Any(details =>
                    details?.Contains("verifiedLinks=2", StringComparison.Ordinal) == true), Is.True);
            });
        }

        var exportResponse = await SendAsync(HttpMethod.Get, "/api/me/data/export", subjectToken);
        exportResponse.EnsureSuccessStatusCode();
        var json = await exportResponse.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("VERIFIED_CONTACT_SENTINEL"));
            Assert.That(json, Does.Contain(subject.Email));
            Assert.That(json, Does.Not.Contain("VERIFIED_APPLICATION_SENTINEL"),
                "guardian perspective must not expose the youth's application narrative");
        });
    }

    [Test]
    public async Task Email_correction_migrates_linked_records_and_later_export_and_removal_keep_full_scope()
    {
        var oldEmail = $"privacy.old.{Guid.NewGuid():N}@example.com";
        var newEmail = $"privacy.new.{Guid.NewGuid():N}@example.com";
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Guardian, oldEmail);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var application = new YouthApplication
        {
            FirstName = "Linked",
            LastName = "Youth",
            Age = 14,
            WhoIsFilling = "guardian",
            Neighbourhood = "Linked neighbourhood",
            CohortInterest = "Linked cohort",
            WhyInterested = "Linked application narrative",
            GuardianName = subject.DisplayName,
            GuardianEmail = oldEmail.ToUpperInvariant(),
            GuardianPhone = "+1 416 555 0110",
            GuardianUserId = subject.Id,
            ParticipationConsentAck = true,
            PrivacyAck = true,
        };
        var contact = new ContactRequest
        {
            Name = subject.DisplayName,
            Email = oldEmail.ToUpperInvariant(),
            Topic = "Linked contact",
            Message = "Linked contact narrative",
            OwnerUserId = subject.Id,
        };
        var volunteer = new VolunteerSignup
        {
            Name = subject.DisplayName,
            Email = oldEmail,
            Phone = "+1 416 555 0111",
            AreasOfInterest = "Linked volunteering",
            ScreeningConsentAck = true,
            OwnerUserId = subject.Id,
        };
        var donation = new DonationPledge
        {
            DonorName = subject.DisplayName,
            Email = oldEmail,
            Tier = "monthly-25",
            MonthlyAmount = 25,
            ProviderReference = "LINKED_PROVIDER_REFERENCE",
            OwnerUserId = subject.Id,
        };
        var invitation = new Invitation
        {
            Email = oldEmail,
            Role = Roles.Youth,
            TokenHash = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(14),
        };
        var outbound = new OutboundEmail
        {
            ToEmail = oldEmail.ToUpperInvariant(),
            Subject = "Linked historical email",
            Body = "Linked historical body",
            SubjectUserId = subject.Id,
            RecipientUserId = subject.Id,
            IdempotencyKey = $"LINKED_DELIVERY_SECRET_{Guid.NewGuid():N}",
            AvailableAt = DateTime.UtcNow.AddDays(30),
        };
        var unlinkedApplication = new YouthApplication
        {
            FirstName = "Unlinked",
            LastName = "Applicant",
            Age = 14,
            WhoIsFilling = "guardian",
            Neighbourhood = "Unlinked neighbourhood",
            CohortInterest = "Unlinked cohort",
            WhyInterested = "UNLINKED_APPLICATION_SENTINEL",
            GuardianName = "Different person",
            GuardianEmail = oldEmail,
            GuardianPhone = "+1 416 555 0198",
            ParticipationConsentAck = true,
            PrivacyAck = true,
        };
        var unlinkedContact = new ContactRequest
        {
            Name = "Different person",
            Email = oldEmail,
            Topic = "UNLINKED_TOPIC_SENTINEL",
            Message = "UNLINKED_CONTACT_SENTINEL",
        };
        var consentRequest = new GuardianConsentRequest
        {
            YouthUserId = Guid.NewGuid(),
            GuardianUserId = subject.Id,
            Destination = oldEmail.ToUpperInvariant(),
            TokenHash = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        var supportRequest = new SupportRequest
        {
            UserId = subject.Id,
            Category = "account",
            Description = "Linked support narrative",
            ReplyToEmail = oldEmail.ToUpperInvariant(),
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AddRange(
                application,
                contact,
                volunteer,
                donation,
                invitation,
                outbound,
                consentRequest,
                supportRequest);
            db.AddRange(unlinkedApplication, unlinkedContact);
            await db.SaveChangesAsync();
        }

        var correctionResponse = await SendAsync(
            HttpMethod.Post,
            "/api/me/data-requests",
            subjectToken,
            new
            {
                actionType = "correction",
                details = "CORRECTION_DETAILS_SENTINEL",
                correction = new
                {
                    displayName = "Corrected linked guardian",
                    email = newEmail,
                    pronouns = "CORRECTION_PRONOUN_SENTINEL",
                },
            });
        correctionResponse.EnsureSuccessStatusCode();
        var correction = await correctionResponse.Content.ReadFromJsonAsync<DataRequestDto>();
        var corrected = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{correction!.Id}/fulfill",
            adminToken,
            new { resolutionNote = "CORRECTION_RESOLUTION_SENTINEL" });
        corrected.EnsureSuccessStatusCode();

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Multiple(() =>
            {
                Assert.That((db.Users.Find(subject.Id))!.Email, Is.EqualTo(newEmail));
                Assert.That((db.Applications.Find(application.Id))!.GuardianEmail, Is.EqualTo(newEmail));
                Assert.That((db.ContactRequests.Find(contact.Id))!.Email, Is.EqualTo(newEmail));
                Assert.That((db.VolunteerSignups.Find(volunteer.Id))!.Email, Is.EqualTo(newEmail));
                Assert.That((db.DonationPledges.Find(donation.Id))!.Email, Is.EqualTo(newEmail));
                Assert.That((db.Invitations.Find(invitation.Id))!.Email, Is.EqualTo(newEmail));
                Assert.That((db.OutboundEmails.Find(outbound.Id))!.ToEmail, Is.EqualTo(newEmail));
                Assert.That((db.GuardianConsentRequests.Find(consentRequest.Id))!.Destination,
                    Is.EqualTo(newEmail));
                Assert.That((db.SupportRequests.Find(supportRequest.Id))!.ReplyToEmail,
                    Is.EqualTo(newEmail));
                Assert.That((db.Applications.Find(unlinkedApplication.Id))!.GuardianEmail,
                    Is.EqualTo(oldEmail));
                Assert.That((db.ContactRequests.Find(unlinkedContact.Id))!.Email,
                    Is.EqualTo(oldEmail));
            });
        }

        var access = await RequestAsync(subjectToken, "access", "ACCESS_DETAILS_SENTINEL");
        var accessFulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{access.Id}/fulfill",
            adminToken,
            new { resolutionNote = "ACCESS_RESOLUTION_SENTINEL" });
        accessFulfilled.EnsureSuccessStatusCode();
        var exportResponse = await SendAsync(
            HttpMethod.Get,
            "/api/me/data/export",
            subjectToken);
        exportResponse.EnsureSuccessStatusCode();
        using (var export = JsonDocument.Parse(await exportResponse.Content.ReadAsStringAsync()))
        {
            var deliveryMetadata = export.RootElement.GetProperty("outboundDeliveryMetadata");
            Assert.Multiple(() =>
            {
                Assert.That(export.RootElement.GetProperty("applicationAndConsent")
                    .GetProperty("applications").GetArrayLength(), Is.EqualTo(1));
                Assert.That(export.RootElement.GetProperty("accountLinkedPublicSubmissions")
                    .GetProperty("contactRequests").GetArrayLength(), Is.EqualTo(1));
                Assert.That(export.RootElement.GetProperty("accountLinkedPublicSubmissions")
                    .GetProperty("volunteerSignups").GetArrayLength(), Is.EqualTo(1));
                Assert.That(export.RootElement.GetProperty("accountLinkedPublicSubmissions")
                    .GetProperty("donationPledges").GetArrayLength(), Is.EqualTo(1));
                Assert.That(export.RootElement.GetProperty("publicSubmissionScope")
                    .GetProperty("unlinkedHandling").GetString(), Does.Contain("separately verify"));
                Assert.That(deliveryMetadata.GetArrayLength(), Is.EqualTo(1));
                Assert.That(deliveryMetadata[0].GetProperty("id").GetGuid(), Is.EqualTo(outbound.Id));
                Assert.That(deliveryMetadata[0].GetProperty("purpose").GetString(),
                    Is.EqualTo("transactional-notification"));
                Assert.That(deliveryMetadata[0].TryGetProperty("toEmail", out _), Is.False);
                Assert.That(deliveryMetadata[0].TryGetProperty("subject", out _), Is.False);
                Assert.That(deliveryMetadata[0].TryGetProperty("body", out _), Is.False);
                Assert.That(deliveryMetadata[0].TryGetProperty("idempotencyKey", out _), Is.False);
                Assert.That(deliveryMetadata[0].ToString(), Does.Not.Contain("Linked historical"));
                Assert.That(deliveryMetadata[0].ToString(), Does.Not.Contain("LINKED_DELIVERY_SECRET"));
            });
        }

        var removal = await RequestAsync(subjectToken, "removal", "REMOVAL_DETAILS_SENTINEL");
        var removedResponse = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{removal.Id}/fulfill",
            adminToken,
            new { resolutionNote = "REMOVAL_RESOLUTION_SENTINEL" });
        removedResponse.EnsureSuccessStatusCode();

        var removalEmail = Factory.Emails.LastTo(newEmail);
        Assert.Multiple(() =>
        {
            Assert.That(removalEmail, Is.Not.Null);
            Assert.That(removalEmail!.SubjectUserId, Is.EqualTo(subject.Id));
            Assert.That(removalEmail.RecipientUserId, Is.Null);
            Assert.That(removalEmail.IsPrivacyRemovalConfirmation, Is.True);
            Assert.That(removalEmail.PurgeAfterDelivery, Is.True,
                "removal-completion envelopes must be erased after their final delivery attempt");
        });

        using var verifyScope = Factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var privacyRequests = await verify.DataSubjectRequests
            .Where(item => item.SubjectUserId == subject.Id)
            .ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(privacyRequests, Has.Count.EqualTo(3));
            Assert.That(privacyRequests, Has.All.Matches<DataSubjectRequest>(item =>
                item.Status == DataSubjectRequestStatus.Completed
                && item.Details is null
                && item.RequestedDisplayName is null
                && item.RequestedEmail is null
                && item.RequestedPronouns is null
                && !item.RequestedClearPronouns
                && item.ResolutionNote is null));
            Assert.That((verify.Applications.Find(application.Id))!.GuardianEmail, Is.Null);
            Assert.That((verify.Applications.Find(application.Id))!.FirstName, Is.EqualTo("Linked"));
            Assert.That((verify.Applications.Find(application.Id))!.WhyInterested,
                Is.EqualTo("Linked application narrative"));
            Assert.That((verify.ContactRequests.Find(contact.Id))!.Email, Does.EndWith("@anonymized.invalid"));
            Assert.That((verify.ContactRequests.Find(contact.Id))!.Topic, Is.EqualTo("[removed]"));
            Assert.That((verify.VolunteerSignups.Find(volunteer.Id))!.Email, Does.EndWith("@anonymized.invalid"));
            Assert.That((verify.DonationPledges.Find(donation.Id))!.Email, Does.EndWith("@anonymized.invalid"));
            Assert.That((verify.Invitations.Find(invitation.Id))!.Email, Does.EndWith("@anonymized.invalid"));
            Assert.That(verify.OutboundEmails.Any(item => item.Id == outbound.Id), Is.False);
            Assert.That(verify.GuardianConsentRequests.Any(item => item.Id == consentRequest.Id), Is.False);
            Assert.That((verify.SupportRequests.Find(supportRequest.Id))!.ReplyToEmail,
                Does.EndWith("@anonymized.invalid"));
            Assert.That((verify.Applications.Find(unlinkedApplication.Id))!.GuardianEmail,
                Is.EqualTo(oldEmail));
            Assert.That((verify.Applications.Find(unlinkedApplication.Id))!.WhyInterested,
                Is.EqualTo("UNLINKED_APPLICATION_SENTINEL"));
            Assert.That((verify.ContactRequests.Find(unlinkedContact.Id))!.Topic,
                Is.EqualTo("UNLINKED_TOPIC_SENTINEL"));
            Assert.That((verify.ContactRequests.Find(unlinkedContact.Id))!.Message,
                Is.EqualTo("UNLINKED_CONTACT_SENTINEL"));
            Assert.That(verify.AuditEntries.Count(item =>
                item.ActorUserId == subject.Id
                && item.Action.StartsWith("DataSubjectRequest:")), Is.EqualTo(3));
        });
    }

    [Test]
    public async Task Concurrent_email_corrections_to_the_same_unique_address_return_one_conflict()
    {
        var (_, firstSubjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (_, secondSubjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (_, firstAdminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var (_, secondAdminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var targetEmail = $"privacy.race.{Guid.NewGuid():N}@example.com";

        async Task<DataRequestDto> CreateCorrectionAsync(string token)
        {
            var response = await SendAsync(
                HttpMethod.Post,
                "/api/me/data-requests",
                token,
                new
                {
                    actionType = "correction",
                    correction = new { email = targetEmail },
                });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<DataRequestDto>())!;
        }

        var first = await CreateCorrectionAsync(firstSubjectToken);
        var second = await CreateCorrectionAsync(secondSubjectToken);
        var attempts = await Task.WhenAll(
            SendAsync(
                HttpMethod.Post,
                $"/api/admin/data-requests/{first.Id}/fulfill",
                firstAdminToken,
                new { }),
            SendAsync(
                HttpMethod.Post,
                $"/api/admin/data-requests/{second.Id}/fulfill",
                secondAdminToken,
                new { }));

        Assert.Multiple(() =>
        {
            Assert.That(attempts.Count(item => item.StatusCode == HttpStatusCode.OK), Is.EqualTo(1));
            Assert.That(attempts.Count(item => item.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1));
        });
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(db.Users.Count(item => item.Email == targetEmail), Is.EqualTo(1));
            Assert.That(db.DataSubjectRequests.Count(item =>
                (item.Id == first.Id || item.Id == second.Id)
                && item.Status == DataSubjectRequestStatus.Completed), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Access_fulfilment_notifies_the_subject_of_the_authenticated_export()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var requested = await RequestAsync(subjectToken, "access", "Send my account data.");

        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            adminToken,
            new { resolutionNote = "Identity verified." });
        Assert.That(fulfilled.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var email = Factory.Emails.LastTo(subject.Email);
        Assert.Multiple(() =>
        {
            Assert.That(email, Is.Not.Null);
            Assert.That(email!.Body, Does.Contain("Sign in to download your comprehensive personal-data export"));
            Assert.That(email.Body, Does.Contain("/settings/privacy"));
            Assert.That(email.Body, Does.Not.Contain(subject.DisplayName));
        });
    }

    [Test]
    public async Task Concurrent_fulfilment_completes_and_notifies_only_once()
    {
        var (subject, subjectToken) = await SeedAndSignInAsync(Roles.Youth);
        var (_, firstAdminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var (_, secondAdminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var requested = await RequestAsync(subjectToken, "correction", "Correct my display name.");

        var attempts = await Task.WhenAll(
            SendAsync(
                HttpMethod.Post,
                $"/api/admin/data-requests/{requested.Id}/fulfill",
                firstAdminToken,
                new { displayName = "First verified name" }),
            SendAsync(
                HttpMethod.Post,
                $"/api/admin/data-requests/{requested.Id}/fulfill",
                secondAdminToken,
                new { displayName = "Second verified name" }));

        Assert.Multiple(() =>
        {
            Assert.That(attempts.Count(response => response.StatusCode == HttpStatusCode.OK), Is.EqualTo(1));
            Assert.That(attempts.Count(response => response.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1));
            Assert.That(Factory.Emails.AllTo(subject.Email), Has.Count.EqualTo(1));
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
    public async Task Staff_removal_preserves_youth_safeguarding_and_learning_records_with_a_pseudonymous_actor()
    {
        const string checkInSentinel = "STAFF_CHECK_IN_SENTINEL";
        const string attendanceSentinel = "STAFF_ATTENDANCE_SENTINEL";
        const string assessmentSentinel = "STAFF_ASSESSMENT_SENTINEL";
        const string flagSentinel = "STAFF_FLAG_SENTINEL";
        const string assignedWorkSentinel = "STAFF_ASSIGNED_WORK_SENTINEL";
        const string staffMessageSentinel = "STAFF_AUTHORED_MESSAGE_SENTINEL";
        const string youthMessageSentinel = "YOUTH_AUTHORED_MESSAGE_SENTINEL";
        var (staff, staffToken) = await SeedAndSignInAsync(Roles.Mentor);
        var (youth, _) = await SeedAndSignInAsync(Roles.Youth);
        var (_, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var checkIn = new CheckIn
        {
            YouthUserId = youth.Id,
            MentorUserId = staff.Id,
            Type = "1:1",
            Engagement = checkInSentinel,
            GoalsDiscussed = checkInSentinel,
            PrayerRequests = checkInSentinel,
            ActionItems = checkInSentinel,
        };
        var attendance = new AttendanceRecord
        {
            SessionId = Guid.NewGuid(),
            YouthUserId = youth.Id,
            Status = AttendanceStatus.Present,
            Note = attendanceSentinel,
            RecordedByUserId = staff.Id,
        };
        var assessment = new CompetencyAssessment
        {
            YouthUserId = youth.Id,
            CompetencyId = Guid.NewGuid(),
            Level = CompetencyLevel.Developing,
            Notes = assessmentSentinel,
            AssessedByUserId = staff.Id,
        };
        var flag = new Flag
        {
            YouthUserId = youth.Id,
            Reason = flagSentinel,
            AutomaticReason = flagSentinel,
            AutomaticSignalKeys = "staff-removal-test",
            ResolutionReason = flagSentinel,
            ResolvedAt = DateTime.UtcNow,
            FlaggedByUserId = staff.Id,
        };
        var work = new AssignedWork
        {
            ItemType = "Lesson",
            ItemId = Guid.NewGuid(),
            YouthUserId = youth.Id,
            Message = assignedWorkSentinel,
            AssignedByUserId = staff.Id,
        };
        var staffMessage = new Message
        {
            ThreadKey = Message.KeyFor(staff.Id, youth.Id),
            SenderId = staff.Id,
            RecipientId = youth.Id,
            Body = staffMessageSentinel,
        };
        var youthMessage = new Message
        {
            ThreadKey = Message.KeyFor(staff.Id, youth.Id),
            SenderId = youth.Id,
            RecipientId = staff.Id,
            Body = youthMessageSentinel,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AddRange(checkIn, attendance, assessment, flag, work, staffMessage, youthMessage);
            await db.SaveChangesAsync();
        }

        var requested = await RequestAsync(staffToken, "removal", "Remove the staff account.");
        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            adminToken,
            new { resolutionNote = "Staff identity verified." });
        Assert.That(fulfilled.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = Factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var removed = await verify.Users.FindAsync(staff.Id);
        Assert.Multiple(() =>
        {
            Assert.That(removed!.Status, Is.EqualTo(UserStatus.Removed));
            Assert.That(removed.DisplayName, Is.EqualTo("Former participant"));
            Assert.That(removed.Email, Does.EndWith("@anonymized.invalid"));
            Assert.That((verify.CheckIns.Find(checkIn.Id))!.MentorUserId, Is.EqualTo(staff.Id));
            Assert.That((verify.CheckIns.Find(checkIn.Id))!.GoalsDiscussed, Is.EqualTo(checkInSentinel));
            Assert.That((verify.CheckIns.Find(checkIn.Id))!.PrayerRequests, Is.EqualTo(checkInSentinel));
            Assert.That((verify.AttendanceRecords.Find(attendance.Id))!.RecordedByUserId, Is.EqualTo(staff.Id));
            Assert.That((verify.AttendanceRecords.Find(attendance.Id))!.Note, Is.EqualTo(attendanceSentinel));
            Assert.That((verify.CompetencyAssessments.Find(assessment.Id))!.AssessedByUserId, Is.EqualTo(staff.Id));
            Assert.That((verify.CompetencyAssessments.Find(assessment.Id))!.Notes, Is.EqualTo(assessmentSentinel));
            Assert.That((verify.Flags.Find(flag.Id))!.FlaggedByUserId, Is.EqualTo(staff.Id));
            Assert.That((verify.Flags.Find(flag.Id))!.Reason, Is.EqualTo(flagSentinel));
            Assert.That((verify.Flags.Find(flag.Id))!.AutomaticReason, Is.EqualTo(flagSentinel));
            Assert.That((verify.Flags.Find(flag.Id))!.ResolutionReason, Is.EqualTo(flagSentinel));
            Assert.That((verify.AssignedWork.Find(work.Id))!.AssignedByUserId, Is.EqualTo(staff.Id));
            Assert.That((verify.AssignedWork.Find(work.Id))!.Message, Is.EqualTo(assignedWorkSentinel));
            Assert.That((verify.Messages.Find(staffMessage.Id))!.Body, Is.EqualTo(staffMessageSentinel));
            Assert.That((verify.Messages.Find(youthMessage.Id))!.Body, Is.EqualTo(youthMessageSentinel));
        });
    }

    [Test]
    public async Task Guardian_only_removal_erases_guardian_identity_without_erasing_child_records()
    {
        const string childApplicationSentinel = "CHILD_APPLICATION_SENTINEL";
        const string safeguardingSentinel = "CHILD_SAFEGUARDING_SENTINEL";
        const string learningSentinel = "CHILD_LEARNING_SENTINEL";
        var (guardian, guardianToken) = await SeedAndSignInAsync(Roles.Guardian);
        var (youth, _) = await SeedAndSignInAsync(Roles.Youth);
        var (admin, adminToken) = await SeedAndSignInAsync(Roles.ProgramAdministrator);
        var application = new YouthApplication
        {
            FirstName = "Child",
            LastName = childApplicationSentinel,
            Age = 14,
            WhoIsFilling = "guardian",
            Neighbourhood = childApplicationSentinel,
            CohortInterest = childApplicationSentinel,
            WhyInterested = childApplicationSentinel,
            GuardianName = guardian.DisplayName,
            GuardianEmail = guardian.Email.ToUpperInvariant(),
            GuardianPhone = "+1 416 555 0102",
            GuardianUserId = guardian.Id,
            ParticipationConsentAck = true,
            PrivacyAck = true,
            CreatedYouthUserId = youth.Id,
        };
        var profile = new GuardianProfile
        {
            GuardianUserId = guardian.Id,
            Relationship = "Parent",
            Phone = "+1 416 555 0103",
            EmergencyContactName = "Private emergency contact",
            EmergencyContactPhone = "+1 416 555 0104",
        };
        var consent = new GuardianConsent
        {
            YouthUserId = youth.Id,
            GuardianUserId = guardian.Id,
            GuardianName = guardian.DisplayName,
            Relationship = "Parent",
            ParticipationConsent = true,
            MediaConsent = true,
            MediaGroupOnly = true,
            Signature = guardian.DisplayName,
            Method = "typed",
            ReferenceId = $"CONSENT-{Guid.NewGuid():N}",
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidUntil = DateTime.UtcNow.AddYears(1),
        };
        var checkIn = new CheckIn
        {
            YouthUserId = youth.Id,
            MentorUserId = admin.Id,
            Type = "1:1",
            GoalsDiscussed = safeguardingSentinel,
            PrayerRequests = safeguardingSentinel,
            ActionItems = safeguardingSentinel,
        };
        var assessment = new CompetencyAssessment
        {
            YouthUserId = youth.Id,
            CompetencyId = Guid.NewGuid(),
            Level = CompetencyLevel.Developing,
            Notes = learningSentinel,
            AssessedByUserId = admin.Id,
        };
        var flag = new Flag
        {
            YouthUserId = youth.Id,
            Reason = safeguardingSentinel,
            FlaggedByUserId = admin.Id,
            ResolvedAt = DateTime.UtcNow,
            ResolutionReason = safeguardingSentinel,
        };
        var work = new AssignedWork
        {
            ItemType = "Lesson",
            ItemId = Guid.NewGuid(),
            YouthUserId = youth.Id,
            Message = learningSentinel,
            AssignedByUserId = admin.Id,
        };
        var progress = new LessonProgress
        {
            YouthUserId = youth.Id,
            LessonId = Guid.NewGuid(),
            Status = ProgressStatus.Completed,
            CompletedAt = DateTime.UtcNow,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var trackedYouth = (await db.Users.FindAsync(youth.Id))!;
            trackedYouth.GuardianUserId = guardian.Id;
            db.AddRange(application, profile, consent, checkIn, assessment, flag, work, progress);
            await db.SaveChangesAsync();
        }

        var requested = await RequestAsync(guardianToken, "removal", "Remove only my guardian data.");
        var fulfilled = await SendAsync(
            HttpMethod.Post,
            $"/api/admin/data-requests/{requested.Id}/fulfill",
            adminToken,
            new { resolutionNote = "Guardian identity verified." });
        Assert.That(fulfilled.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = Factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var removedGuardian = await verify.Users.FindAsync(guardian.Id);
        var preservedApplication = (await verify.Applications.FindAsync(application.Id))!;
        var preservedConsent = (await verify.GuardianConsents.FindAsync(consent.Id))!;
        Assert.Multiple(() =>
        {
            Assert.That(removedGuardian!.Status, Is.EqualTo(UserStatus.Removed));
            Assert.That(removedGuardian.DisplayName, Is.EqualTo("Former participant"));
            Assert.That(removedGuardian.Email, Does.EndWith("@anonymized.invalid"));
            Assert.That(verify.GuardianProfiles.Any(item => item.Id == profile.Id), Is.False);
            Assert.That(preservedApplication.FirstName, Is.EqualTo("Child"));
            Assert.That(preservedApplication.LastName, Is.EqualTo(childApplicationSentinel));
            Assert.That(preservedApplication.WhyInterested, Is.EqualTo(childApplicationSentinel));
            Assert.That(preservedApplication.CreatedYouthUserId, Is.EqualTo(youth.Id));
            Assert.That(preservedApplication.GuardianName, Is.Null);
            Assert.That(preservedApplication.GuardianEmail, Is.Null);
            Assert.That(preservedApplication.GuardianPhone, Is.Null);
            Assert.That(preservedConsent.YouthUserId, Is.EqualTo(youth.Id));
            Assert.That(preservedConsent.GuardianUserId, Is.Null);
            Assert.That(preservedConsent.GuardianName, Is.EqualTo("[removed]"));
            Assert.That(preservedConsent.Signature, Is.EqualTo("[removed]"));
            Assert.That(preservedConsent.ParticipationConsent, Is.True);
            Assert.That(preservedConsent.MediaConsent, Is.True);
            Assert.That(preservedConsent.MediaGroupOnly, Is.True);
            Assert.That(preservedConsent.WithdrawnAt, Is.Null,
                "account removal is not a consent-withdrawal request for the child");
            Assert.That((verify.CheckIns.Find(checkIn.Id))!.GoalsDiscussed, Is.EqualTo(safeguardingSentinel));
            Assert.That((verify.CompetencyAssessments.Find(assessment.Id))!.Notes, Is.EqualTo(learningSentinel));
            Assert.That((verify.Flags.Find(flag.Id))!.Reason, Is.EqualTo(safeguardingSentinel));
            Assert.That((verify.AssignedWork.Find(work.Id))!.Message, Is.EqualTo(learningSentinel));
            Assert.That((verify.LessonProgress.Find(progress.Id))!.Status, Is.EqualTo(ProgressStatus.Completed));
            Assert.That((verify.Users.Find(youth.Id))!.Status, Is.EqualTo(UserStatus.Active));
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

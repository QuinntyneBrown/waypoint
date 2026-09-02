using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-003 (submission grading), L2-MENTOR-004 (quiz approval/override),
//            L2-LEARN-005 (quiz-passed auto-badge on approval)
[TestFixture]
[Property("Req", "L2-MENTOR-003")]
public sealed class GradingTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<string> SignInAsync(string email)
    {
        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<Waypoint.Application.Authentication.Auth.AuthResult>();
        return auth!.AccessToken;
    }

    // Seeds a mentor + a youth assigned to that mentor. Returns their ids and signed-in tokens.
    private async Task<(Guid mentorId, string mentorToken, Guid youthId, string youthToken)> SeedMentorAndYouthAsync()
    {
        var mentorEmail = $"mentor.{Guid.NewGuid():N}@example.com";
        var youthEmail = $"youth.{Guid.NewGuid():N}@example.com";
        Guid mentorId, youthId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var mentor = new User
            {
                Email = mentorEmail,
                DisplayName = "Mentor",
                Role = Roles.Mentor,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            var guardian = new User
            {
                Email = $"guardian.{Guid.NewGuid():N}@example.com",
                DisplayName = "Guardian",
                Role = Roles.Guardian,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            db.Users.AddRange(mentor, guardian);
            await db.SaveChangesAsync();
            mentorId = mentor.Id;

            var youth = new User
            {
                Email = youthEmail,
                DisplayName = "Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                MentorId = mentorId,
                GuardianUserId = guardian.Id,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            db.Users.Add(youth);
            db.GuardianConsents.Add(new GuardianConsent
            {
                YouthUserId = youth.Id,
                GuardianUserId = guardian.Id,
                GuardianName = guardian.DisplayName,
                Relationship = "Parent",
                ParticipationConsent = true,
                Signature = guardian.DisplayName,
                ReferenceId = $"GRADING-{Guid.NewGuid():N}",
                SignedAt = DateTime.UtcNow.AddDays(-1),
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidUntil = DateTime.UtcNow.AddMonths(6),
            });
            await db.SaveChangesAsync();
            youthId = youth.Id;
        }

        return (mentorId, await SignInAsync(mentorEmail), youthId, await SignInAsync(youthEmail));
    }

    private sealed record SeededSubmission(Guid Id, Guid CriterionId, string BadgeName);
    private sealed record MentorSubmissionDetailDto(
        Guid Id,
        Guid YouthId,
        string YouthName,
        string AssignmentTitle,
        string Status,
        string Version,
        string? BodyText,
        string? PhotoUrl,
        string? LinkUrl,
        DateTime SubmittedAt,
        string? MentorComment,
        int? TotalAwarded,
        int TotalPossible,
        IReadOnlyList<MentorRubricCriterionDto> Rubric);
    private sealed record MentorRubricCriterionDto(
        Guid CriterionId,
        string Description,
        int PossiblePoints,
        int? AwardedPoints);
    private sealed record MentorQuizReviewDto(
        Guid AttemptId,
        Guid YouthId,
        string YouthName,
        Guid QuizId,
        string QuizTitle,
        int AttemptNumber,
        int AutoScore,
        int? ApprovedScore,
        int PassMark,
        bool AutoPassed,
        bool? ApprovedPassed,
        string Status,
        DateTime SubmittedAt,
        DateTime? ApprovedAt,
        string? ApprovedByName,
        IReadOnlyList<MentorQuizQuestionDto> Questions);
    private sealed record MentorQuizQuestionDto(
        Guid QuestionId,
        string Text,
        IReadOnlyList<string> Options,
        IReadOnlyList<int> SelectedIndexes,
        IReadOnlyList<int> CorrectIndexes,
        bool IsCorrect);

    private async Task<SeededSubmission> SeedSubmissionAsync(Guid youthId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var assignment = new Assignment
        {
            Title = "Reflection",
            TaskDescription = "Reflect on the lesson.",
            SubmissionTypes = "text",
            TotalPoints = 10,
        };
        db.Assignments.Add(assignment);
        var criterion = new RubricCriterion
        {
            AssignmentId = assignment.Id,
            Description = "Depth of reflection",
            Points = 10,
            Order = 1,
        };
        db.RubricCriteria.Add(criterion);
        var badge = new Badge
        {
            Name = $"Assignment badge {Guid.NewGuid():N}",
            Icon = "star",
            CriteriaKind = BadgeCriteriaKind.Auto,
            AutoRule = "assignment-approved",
            TargetRefId = assignment.Id,
        };
        db.Badges.Add(badge);
        var submission = new Submission
        {
            YouthUserId = youthId,
            AssignmentId = assignment.Id,
            BodyText = "My work",
            Status = SubmissionStatus.Submitted,
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();
        return new SeededSubmission(submission.Id, criterion.Id, badge.Name);
    }

    [Test]
    [Property("Req", "L2-MENTOR-003")]
    public async Task Review_detail_enforces_rubric_bounds_computes_total_and_retains_a_draft()
    {
        // Traces to: L2-MENTOR-003 AC1/AC4/AC5
        var (_, mentorToken, youthId, _) = await SeedMentorAndYouthAsync();
        var submission = await SeedSubmissionAsync(youthId);

        var detailResponse = await GetAsync($"/api/mentor/submissions/{submission.Id}", mentorToken);
        Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await detailResponse.Content.ReadFromJsonAsync<MentorSubmissionDetailDto>();
        Assert.Multiple(() =>
        {
            Assert.That(detail!.YouthName, Is.EqualTo("Youth"));
            Assert.That(detail.AssignmentTitle, Is.EqualTo("Reflection"));
            Assert.That(detail.BodyText, Is.EqualTo("My work"));
            Assert.That(detail.Rubric.Single().PossiblePoints, Is.EqualTo(10));
        });

        var outsideRange = await PostAsync(
            $"/api/mentor/submissions/{submission.Id}/grade",
            new
            {
                decision = "save-draft",
                rubricScoresJson = JsonSerializer.Serialize(new Dictionary<string, int>
                {
                    [submission.CriterionId.ToString()] = 11,
                }),
                totalAwarded = 11,
                guardianNotified = true,
                expectedVersion = detail!.Version,
            },
            mentorToken);
        Assert.That(outsideRange.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var incompleteApproval = await PostAsync(
            $"/api/mentor/submissions/{submission.Id}/grade",
            new
            {
                decision = "approve",
                rubricScoresJson = JsonSerializer.Serialize(new Dictionary<string, int>()),
                totalAwarded = 999,
                guardianNotified = true,
                expectedVersion = detail!.Version,
            },
            mentorToken);
        Assert.That(incompleteApproval.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var draft = await PostAsync(
            $"/api/mentor/submissions/{submission.Id}/grade",
            new
            {
                decision = "save-draft",
                rubricScoresJson = JsonSerializer.Serialize(new Dictionary<string, int>
                {
                    [submission.CriterionId.ToString()] = 7,
                }),
                totalAwarded = 999,
                comment = "Draft feedback",
                guardianNotified = true,
                expectedVersion = detail!.Version,
            },
            mentorToken);
        Assert.That(draft.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterDraft = await GetAsync($"/api/mentor/submissions/{submission.Id}", mentorToken);
        var saved = await afterDraft.Content.ReadFromJsonAsync<MentorSubmissionDetailDto>();
        Assert.Multiple(() =>
        {
            Assert.That(saved!.Status, Is.EqualTo(SubmissionStatus.Submitted.ToString()));
            Assert.That(saved.TotalAwarded, Is.EqualTo(7), "the server computes the rubric total");
            Assert.That(saved.MentorComment, Is.EqualTo("Draft feedback"));
            Assert.That(saved.Rubric.Single().AwardedPoints, Is.EqualTo(7));
        });
    }

    [Test]
    [Property("Req", "L2-CURRIC-005")]
    [Property("Req", "L2-MENTOR-003")]
    public async Task Assigned_mentor_approves_but_an_unassigned_mentor_is_forbidden()
    {
        var (_, mentorToken, youthId, youthToken) = await SeedMentorAndYouthAsync();
        var submission = await SeedSubmissionAsync(youthId);
        var reviewResponse = await GetAsync($"/api/mentor/submissions/{submission.Id}", mentorToken);
        var review = await reviewResponse.Content.ReadFromJsonAsync<MentorSubmissionDetailDto>();

        // Traces to: L2-MENTOR-003 (mentor approves -> Approved + points visible to youth)
        var grade = await PostAsync(
            $"/api/mentor/submissions/{submission.Id}/grade",
            new
            {
                decision = "approve",
                rubricScoresJson = JsonSerializer.Serialize(new Dictionary<string, int>
                {
                    [submission.CriterionId.ToString()] = 9,
                }),
                totalAwarded = 9,
                comment = "Great work",
                guardianNotified = false,
                expectedVersion = review!.Version,
            },
            mentorToken);
        Assert.That(
            grade.StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent),
            Factory.ExceptionAlerts.Alerts.LastOrDefault()?.ExceptionType);

        var status = await GetAsync($"/api/me/submissions/{submission.Id}", youthToken);
        var view = await status.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.Multiple(() =>
        {
            Assert.That(view!.Status, Is.EqualTo(SubmissionStatus.Approved.ToString()));
            Assert.That(view.TotalAwarded, Is.EqualTo(9));
            Assert.That(view.MentorComment, Is.EqualTo("Great work"));
            Assert.That(view.Rubric[0].AwardedPoints, Is.EqualTo(9));
            Assert.That(view.EarnedBadge!.Name, Is.EqualTo(submission.BadgeName));
            Assert.That(view.Timeline.Select(item => item.Type), Does.Contain("Approved"));
            Assert.That(view.BodyText, Is.EqualTo("My work"));
        });
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(await db.Notifications.AnyAsync(item =>
                item.UserId == youthId
                && item.EventKey == "work-graded"
                && item.LinkPath == $"/youth/submissions/{submission.Id}"), Is.True);
            var guardianId = await db.Users
                .Where(item => item.Id == youthId)
                .Select(item => item.GuardianUserId)
                .SingleAsync();
            Assert.That(await db.Notifications.AnyAsync(item =>
                item.UserId == guardianId
                && item.EventKey == "work-graded"), Is.False,
                "the guardian opted out of this outcome notification");
            Assert.That(await db.BadgeAwards.CountAsync(item =>
                item.YouthUserId == youthId), Is.EqualTo(1));
            Assert.That(await db.Notifications.CountAsync(item =>
                item.UserId == youthId && item.EventKey == "badge-earned"), Is.EqualTo(1));
            Assert.That(Factory.Changes.Changes.Count(item =>
                item.Audience == "user"
                && item.AudienceId == youthId
                && item.EventName == "notification"
                && JsonSerializer.Serialize(item.Payload).Contains("badge-earned")), Is.EqualTo(1));
        }

        // Traces to: L2-MENTOR-003 AC5 (a mentor not assigned to this youth cannot grade -> 403)
        var strangerToken = await CreateUserAndGetTokenAsync(Roles.Mentor);
        var otherSubmission = await SeedSubmissionAsync(youthId);
        var forbidden = await PostAsync(
            $"/api/mentor/submissions/{otherSubmission.Id}/grade",
            new
            {
                decision = "approve",
                totalAwarded = 5,
                guardianNotified = false,
                expectedVersion = Guid.NewGuid().ToString(),
            },
            strangerToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-MENTOR-003")]
    [Property("Req", "L2-LEARN-007")]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Request_revision_requires_a_comment_and_the_youth_can_then_resubmit()
    {
        // Traces to: L2-MENTOR-003; L2-LEARN-007 AC3; L2-ENGAGE-008; L2-PLATFORM-003 AC2
        var (mentorId, mentorToken, youthId, youthToken) = await SeedMentorAndYouthAsync();
        var submission = await SeedSubmissionAsync(youthId);
        string mentorEmail;
        using (var preferenceScope = Factory.Services.CreateScope())
        {
            var preferenceDb = preferenceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            mentorEmail = await preferenceDb.Users
                .Where(item => item.Id == mentorId)
                .Select(item => item.Email)
                .SingleAsync();
            preferenceDb.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = mentorId,
                EventKey = "approval-needed",
                InApp = false,
                Email = true,
            });
            await preferenceDb.SaveChangesAsync();
        }
        var reviewResponse = await GetAsync($"/api/mentor/submissions/{submission.Id}", mentorToken);
        var review = await reviewResponse.Content.ReadFromJsonAsync<MentorSubmissionDetailDto>();

        // Traces to: L2-MENTOR-003 AC2 (request-revision without a comment -> 400)
        var noComment = await PostAsync(
            $"/api/mentor/submissions/{submission.Id}/grade",
            new
            {
                decision = "request-revision",
                guardianNotified = true,
                expectedVersion = review!.Version,
            },
            mentorToken);
        Assert.That(noComment.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var withComment = await PostAsync(
            $"/api/mentor/submissions/{submission.Id}/grade",
            new
            {
                decision = "request-revision",
                comment = "Please add scripture.",
                guardianNotified = true,
                expectedVersion = review!.Version,
            },
            mentorToken);
        Assert.That(withComment.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var guardianId = await db.Users
                .Where(item => item.Id == youthId)
                .Select(item => item.GuardianUserId)
                .SingleAsync();
            Assert.That(await db.Notifications.AnyAsync(item =>
                item.UserId == guardianId
                && item.EventKey == "work-graded"
                && item.LinkPath == "/guardian/progress"), Is.True);
        }

        var afterRevision = await GetAsync($"/api/me/submissions/{submission.Id}", youthToken);
        var revView = await afterRevision.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.That(revView!.Status, Is.EqualTo(SubmissionStatus.NeedsRevision.ToString()));

        // Traces to: L2-LEARN-007 AC3 (youth resubmits -> UnderReview)
        var resubmit = await PostAsync(
            $"/api/me/submissions/{submission.Id}/resubmit",
            new { bodyText = "Now with scripture", expectedVersion = revView.Version },
            youthToken);
        Assert.That(resubmit.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var approvalEmail = Factory.Emails.LastTo(mentorEmail);
        Assert.Multiple(() =>
        {
            Assert.That(approvalEmail, Is.Not.Null, "the assigned mentor receives the renewed review request");
            Assert.That(approvalEmail!.Subject, Is.EqualTo("Revised assignment ready for review"));
            Assert.That(approvalEmail.Body, Does.Contain($"/mentor/reviews/submissions/{submission.Id}"));
            Assert.That(Factory.Changes.Changes.Any(item =>
                item.Audience == "user"
                && item.AudienceId == mentorId
                && item.EventName == "notification"
                && JsonSerializer.Serialize(item.Payload).Contains("approval-needed")), Is.False);
        });

        var afterResubmit = await GetAsync($"/api/me/submissions/{submission.Id}", youthToken);
        var reView = await afterResubmit.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.Multiple(() =>
        {
            Assert.That(reView!.Status, Is.EqualTo(SubmissionStatus.UnderReview.ToString()));
            Assert.That(reView.BodyText, Is.EqualTo("Now with scripture"));
            Assert.That(reView.Timeline.Select(item => item.Type), Does.Contain("NeedsRevision"));
            Assert.That(reView.Timeline.Select(item => item.Type), Does.Contain("Resubmitted"));
        });
    }

    [Test]
    [Property("Req", "L2-MENTOR-003")]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Graded_work_uses_the_canonical_preference_for_in_app_and_email_delivery()
    {
        // Traces to: L2-MENTOR-003, L2-ENGAGE-008 AC1/AC4, L2-PLATFORM-003 AC2
        var (_, mentorToken, youthId, _) = await SeedMentorAndYouthAsync();
        var submission = await SeedSubmissionAsync(youthId);
        string youthEmail;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            youthEmail = await db.Users
                .Where(item => item.Id == youthId)
                .Select(item => item.Email)
                .SingleAsync();
            db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = youthId,
                EventKey = "work-graded",
                InApp = false,
                Email = true,
            });
            await db.SaveChangesAsync();
        }

        var reviewResponse = await GetAsync($"/api/mentor/submissions/{submission.Id}", mentorToken);
        var review = await reviewResponse.Content.ReadFromJsonAsync<MentorSubmissionDetailDto>();
        var grade = await PostAsync(
            $"/api/mentor/submissions/{submission.Id}/grade",
            new
            {
                decision = "request-revision",
                comment = "Please connect this reflection to scripture.",
                guardianNotified = false,
                expectedVersion = review!.Version,
            },
            mentorToken);
        Assert.That(grade.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Multiple(() =>
            {
                Assert.That(db.Notifications.Any(item =>
                    item.UserId == youthId && item.EventKey == "work-graded"), Is.False);
                Assert.That(db.Notifications.Any(item =>
                    item.UserId == youthId && item.EventKey == "submission-status"), Is.False);
                Assert.That(Factory.Changes.Changes.Any(item =>
                    item.Audience == "user"
                    && item.AudienceId == youthId
                    && item.EventName == "notification"), Is.False);
            });
        }
        var email = Factory.Emails.LastTo(youthEmail);
        Assert.Multiple(() =>
        {
            Assert.That(email, Is.Not.Null);
            Assert.That(email!.Subject, Does.Contain("needs revision").IgnoreCase);
            Assert.That(email.Body, Does.Contain($"/youth/submissions/{submission.Id}"));
        });
    }

    [Test]
    [Property("Req", "L2-CURRIC-005")]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Badge_earned_uses_its_preference_for_in_app_and_email_delivery()
    {
        // Traces to: L2-CURRIC-005, L2-ENGAGE-008 AC1/AC4, L2-PLATFORM-003 AC2
        var (_, mentorToken, youthId, _) = await SeedMentorAndYouthAsync();
        var submission = await SeedSubmissionAsync(youthId);
        string youthEmail;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            youthEmail = await db.Users
                .Where(item => item.Id == youthId)
                .Select(item => item.Email)
                .SingleAsync();
            db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = youthId,
                EventKey = "badge-earned",
                InApp = false,
                Email = true,
            });
            await db.SaveChangesAsync();
        }

        var reviewResponse = await GetAsync($"/api/mentor/submissions/{submission.Id}", mentorToken);
        var review = await reviewResponse.Content.ReadFromJsonAsync<MentorSubmissionDetailDto>();
        var grade = await PostAsync(
            $"/api/mentor/submissions/{submission.Id}/grade",
            new
            {
                decision = "approve",
                rubricScoresJson = JsonSerializer.Serialize(new Dictionary<string, int>
                {
                    [submission.CriterionId.ToString()] = 10,
                }),
                comment = "Excellent work.",
                guardianNotified = false,
                expectedVersion = review!.Version,
            },
            mentorToken);
        Assert.That(grade.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Multiple(() =>
            {
                Assert.That(db.BadgeAwards.Any(item =>
                    item.YouthUserId == youthId), Is.True);
                Assert.That(db.Notifications.Any(item =>
                    item.UserId == youthId && item.EventKey == "badge-earned"), Is.False);
                Assert.That(Factory.Changes.Changes.Any(item =>
                    item.Audience == "user"
                    && item.AudienceId == youthId
                    && item.EventName == "notification"
                    && JsonSerializer.Serialize(item.Payload).Contains("badge-earned")), Is.False);
            });
        }
        var email = Factory.Emails.LastTo(youthEmail);
        Assert.Multiple(() =>
        {
            Assert.That(email, Is.Not.Null);
            Assert.That(email!.Subject, Does.Contain("badge earned").IgnoreCase);
            Assert.That(email.Body, Does.Contain("/youth/journey"));
        });
    }

    [Test]
    [Property("Req", "L2-CURRIC-005")]
    [Property("Req", "L2-MENTOR-004")]
    [Property("Req", "L2-LEARN-005")]
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Approving_a_passing_quiz_attempt_records_the_score_and_awards_the_quiz_badge()
    {
        var (_, mentorToken, youthId, _) = await SeedMentorAndYouthAsync();

        Guid attemptId, badgeId, moduleBadgeId;
        string youthEmail;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var track = new Track
            {
                Name = "Quiz Track",
                Key = $"quiz-{Guid.NewGuid():N}",
                Status = PublishStatus.Published,
            };
            var module = new Module
            {
                TrackId = track.Id,
                Name = "Quiz Module",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var quiz = new Quiz
            {
                ModuleId = module.Id,
                PassMark = 70,
                AttemptsAllowed = 2,
                RequiredToCompleteModule = true,
            };
            db.Tracks.Add(track);
            db.Modules.Add(module);
            db.Quizzes.Add(quiz);
            var attempt = new QuizAttempt
            {
                YouthUserId = youthId,
                QuizId = quiz.Id,
                AttemptNumber = 1,
                AutoScore = 80,
                Passed = true,
                IsSubmitted = true,
                SubmittedAt = DateTime.UtcNow,
            };
            db.QuizAttempts.Add(attempt);
            var badge = new Badge
            {
                Name = "Quiz Ace",
                Icon = "trophy",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "quiz-passed",
                TargetRefId = quiz.Id,
            };
            var moduleBadge = new Badge
            {
                Name = "Quiz Module Finisher",
                Icon = "award",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "module-completed",
                TargetRefId = module.Id,
            };
            db.Badges.AddRange(badge, moduleBadge);
            db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = youthId,
                EventKey = "badge-earned",
                InApp = false,
                Email = true,
            });
            await db.SaveChangesAsync();
            attemptId = attempt.Id;
            badgeId = badge.Id;
            moduleBadgeId = moduleBadge.Id;
            youthEmail = await db.Users
                .Where(item => item.Id == youthId)
                .Select(item => item.Email)
                .SingleAsync();
        }

        // Traces to: L2-MENTOR-004 (mentor approves the attempt)
        var approve = await PostAsync($"/api/mentor/attempts/{attemptId}/approve", new { }, mentorToken);
        Assert.That(approve.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Traces to: L2-MENTOR-004 AC4 + L2-LEARN-005 AC2 (approved score recorded; badge awarded)
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var attempt = await db.QuizAttempts.FirstAsync(a => a.Id == attemptId);
            Assert.That(attempt.ApprovedScore, Is.EqualTo(80));
            Assert.That(attempt.ApprovedAt, Is.Not.Null);

            var expectedBadgeIds = new List<Guid> { badgeId, moduleBadgeId };
            var awards = await db.BadgeAwards
                .Where(item => item.YouthUserId == youthId && expectedBadgeIds.Contains(item.BadgeId))
                .Select(item => item.BadgeId)
                .ToListAsync();
            var notifications = await db.Notifications
                .CountAsync(item => item.UserId == youthId && item.EventKey == "badge-earned");
            var liveNotifications = Factory.Changes.Changes.Count(item =>
                item.Audience == "user"
                && item.AudienceId == youthId
                && item.EventName == "notification"
                && JsonSerializer.Serialize(item.Payload).Contains("badge-earned"));
            var emails = Factory.Emails.AllTo(youthEmail);
            Assert.Multiple(() =>
            {
                Assert.That(awards, Is.EquivalentTo(expectedBadgeIds),
                    "quiz approval awards both the quiz and newly completed module badges");
                Assert.That(awards, Has.Count.EqualTo(2), "each badge is awarded exactly once");
                Assert.That(notifications, Is.Zero, "the disabled in-app channel is suppressed");
                Assert.That(liveNotifications, Is.Zero, "the disabled in-app channel is not pushed live");
                Assert.That(emails.Select(item => item.Subject), Is.EquivalentTo(new[]
                {
                    "Badge earned: Quiz Ace",
                    "Badge earned: Quiz Module Finisher",
                }));
                Assert.That(emails.All(item => item.Body.Contains("/youth/journey")), Is.True);
            });
        }
    }

    [Test]
    [Property("Req", "L2-MENTOR-004")]
    [Property("Req", "L2-LEARN-005")]
    public async Task Quiz_review_is_scoped_and_retains_auto_and_overridden_scores_for_audit()
    {
        // Traces to: L2-MENTOR-004 AC1/AC2/AC4, L2-LEARN-005 AC2
        var (_, mentorToken, youthId, youthToken) = await SeedMentorAndYouthAsync();
        Guid attemptId;
        Guid questionId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var quiz = new Quiz { PassMark = 70, AttemptsAllowed = 2 };
            var question = new QuizQuestion
            {
                QuizId = quiz.Id,
                Text = "Which response shows service?",
                Type = QuestionType.MultipleChoice,
                Order = 1,
                OptionsJson = JsonSerializer.Serialize(new[] { "Walk away", "Offer help" }),
                CorrectAnswersJson = JsonSerializer.Serialize(new[] { 1 }),
            };
            var attempt = new QuizAttempt
            {
                YouthUserId = youthId,
                QuizId = quiz.Id,
                AttemptNumber = 1,
                AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, int[]>
                {
                    [question.Id] = [0],
                }),
                AutoScore = 0,
                Passed = false,
                IsSubmitted = true,
                SubmittedAt = DateTime.UtcNow,
            };
            db.Quizzes.Add(quiz);
            db.QuizQuestions.Add(question);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
            attemptId = attempt.Id;
            questionId = question.Id;
        }

        var detailResponse = await GetAsync($"/api/mentor/attempts/{attemptId}", mentorToken);
        Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await detailResponse.Content.ReadFromJsonAsync<MentorQuizReviewDto>();
        Assert.Multiple(() =>
        {
            Assert.That(detail!.YouthName, Is.EqualTo("Youth"));
            Assert.That(detail.QuizTitle, Is.EqualTo("Quiz"));
            Assert.That(detail.AutoScore, Is.EqualTo(0));
            Assert.That(detail.ApprovedScore, Is.Null);
            Assert.That(detail.Status, Is.EqualTo("Pending"));
            Assert.That(detail.Questions.Single().QuestionId, Is.EqualTo(questionId));
            Assert.That(detail.Questions.Single().SelectedIndexes, Is.EqualTo(new[] { 0 }));
            Assert.That(detail.Questions.Single().CorrectIndexes, Is.EqualTo(new[] { 1 }));
        });

        var strangerToken = await CreateUserAndGetTokenAsync(Roles.Mentor);
        var forbidden = await GetAsync($"/api/mentor/attempts/{attemptId}", strangerToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var outsideRange = await PostAsync(
            $"/api/mentor/attempts/{attemptId}/approve",
            new { overrideScore = 101 },
            mentorToken);
        Assert.That(outsideRange.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var approve = await PostAsync(
            $"/api/mentor/attempts/{attemptId}/approve",
            new { overrideScore = 75 },
            mentorToken);
        Assert.That(approve.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var repeatApproval = await PostAsync(
            $"/api/mentor/attempts/{attemptId}/approve",
            new { overrideScore = 90 },
            mentorToken);
        Assert.That(repeatApproval.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var approvedResponse = await GetAsync($"/api/mentor/attempts/{attemptId}", mentorToken);
        var approved = await approvedResponse.Content.ReadFromJsonAsync<MentorQuizReviewDto>();
        Assert.Multiple(() =>
        {
            Assert.That(approved!.AutoScore, Is.EqualTo(0));
            Assert.That(approved.ApprovedScore, Is.EqualTo(75));
            Assert.That(approved.ApprovedPassed, Is.True);
            Assert.That(approved.Status, Is.EqualTo("Approved"));
            Assert.That(approved.ApprovedAt, Is.Not.Null);
            Assert.That(approved.ApprovedByName, Is.EqualTo("Mentor"));
        });

        var youthResult = await GetAsync($"/api/me/attempts/{attemptId}", youthToken);
        var resultJson = await youthResult.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(resultJson.GetProperty("score").GetInt32(), Is.EqualTo(75));
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var auditDetails = (await db.AuditEntries
                    .Where(entry =>
                        entry.Action == "QuizApproved"
                        && entry.TargetId == attemptId)
                    .ToListAsync())
                .Select(entry => entry.Details)
                .ToList();
            Assert.That(auditDetails, Is.EquivalentTo(new[] { "auto=0;override=75" }));
        }
    }

    [Test]
    [Property("Req", "L2-MENTOR-004")]
    public async Task Approve_all_is_scoped_and_records_every_pending_assessment()
    {
        // Traces to: L2-MENTOR-004 AC3/AC4
        var (_, mentorToken, youthId, _) = await SeedMentorAndYouthAsync();
        var (_, _, otherYouthId, _) = await SeedMentorAndYouthAsync();
        Guid firstId;
        Guid secondId;
        Guid outsiderId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var quiz = new Quiz { PassMark = 70 };
            db.Quizzes.Add(quiz);
            var attempts = new[]
            {
                new QuizAttempt
                {
                    YouthUserId = youthId,
                    QuizId = quiz.Id,
                    AttemptNumber = 1,
                    AutoScore = 80,
                    IsSubmitted = true,
                    SubmittedAt = DateTime.UtcNow,
                },
                new QuizAttempt
                {
                    YouthUserId = youthId,
                    QuizId = quiz.Id,
                    AttemptNumber = 2,
                    AutoScore = 60,
                    IsSubmitted = true,
                    SubmittedAt = DateTime.UtcNow,
                },
                new QuizAttempt
                {
                    YouthUserId = otherYouthId,
                    QuizId = quiz.Id,
                    AttemptNumber = 1,
                    AutoScore = 90,
                    IsSubmitted = true,
                    SubmittedAt = DateTime.UtcNow,
                },
            };
            db.QuizAttempts.AddRange(attempts);
            await db.SaveChangesAsync();
            firstId = attempts[0].Id;
            secondId = attempts[1].Id;
            outsiderId = attempts[2].Id;
        }

        var approveAll = await PostAsync("/api/mentor/attempts/approve-all", new { }, mentorToken);
        Assert.That(approveAll.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await approveAll.Content.ReadFromJsonAsync<int>(), Is.EqualTo(2));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Multiple(() =>
            {
                Assert.That(db.QuizAttempts.Single(item => item.Id == firstId).ApprovedScore, Is.EqualTo(80));
                Assert.That(db.QuizAttempts.Single(item => item.Id == secondId).ApprovedScore, Is.EqualTo(60));
                Assert.That(db.QuizAttempts.Single(item => item.Id == outsiderId).ApprovedScore, Is.Null);
                Assert.That(db.AuditEntries.Count(entry =>
                    entry.Action == "QuizApproved"
                    && (entry.TargetId == firstId || entry.TargetId == secondId)), Is.EqualTo(2));
            });
        }
    }

    private sealed record RubricScoreDto(
        Guid CriterionId,
        string Description,
        int PossiblePoints,
        int? AwardedPoints);

    private sealed record TimelineDto(string Type, string Label, DateTime? At, bool Current);
    private sealed record BadgeDto(Guid Id, string Name, string Icon, string? Description, DateTime AwardedAt);

    private sealed record SubmissionStatusDto(
        Guid Id,
        Guid AssignmentId,
        string AssignmentTitle,
        string Status,
        string Version,
        string? BodyText,
        string? PhotoUrl,
        string? LinkUrl,
        string? MentorComment,
        int? TotalAwarded,
        int TotalPossible,
        DateTime SubmittedAt,
        DateTime? ReviewedAt,
        List<TimelineDto> Timeline,
        List<RubricScoreDto> Rubric,
        BadgeDto? EarnedBadge);
}

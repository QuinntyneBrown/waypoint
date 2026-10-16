using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Waypoint.Api.Authorization;
using Waypoint.Api.Middleware;
using Waypoint.Api.Observability;
using Waypoint.Api.Realtime;
using Waypoint.Api.Security;
using Waypoint.Application;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Common;
using Waypoint.Infrastructure;
using Waypoint.Infrastructure.Health;

namespace Waypoint.Api.TestSupport;

/// <summary>Testing-environment-only seed endpoints used by the Playwright acceptance harness.</summary>
internal static class TestSupportEndpoints
{
    public static void MapTestSupportEndpoints(this WebApplication app)
    {
        app.MapPost("/api/_test/users", async (
            Waypoint.Api.Contracts.TestSupport.SeedUserBody body,
            IAppDbContext db,
            IPasswordHasher hasher,
            CancellationToken cancellationToken) =>
        {
            if (!Roles.IsValid(body.Role))
            {
                return Results.BadRequest(new { title = $"Unknown role '{body.Role}'." });
            }

            var user = new User
            {
                Email = body.Email,
                DisplayName = body.DisplayName,
                Role = body.Role,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(body.Password),
                OnboardingCompleted = true,
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/_test/users/{user.Id}", new { userId = user.Id });
        });

        // Volunteer attendance E2E requires an assigned same-day session with a populated cohort. Session
        // assignment is not otherwise exposed by the product API yet, so keep this fixture behind Testing.
        app.MapPost("/api/_test/volunteer-sessions", async (
            Waypoint.Api.Contracts.TestSupport.SeedVolunteerSessionBody body,
            IAppDbContext db,
            IClock clock,
            CancellationToken cancellationToken) =>
        {
            var volunteer = await db.Users.FirstOrDefaultAsync(
                u => u.Id == body.VolunteerUserId && u.Role == Roles.Volunteer,
                cancellationToken);
            if (volunteer is null)
            {
                return Results.NotFound(new { title = "Volunteer not found." });
            }

            var suffix = Guid.NewGuid().ToString("N")[..8];
            var today = clock.UtcNow.Date;
            var cohort = new Cohort
            {
                Name = $"E2E Volunteer Cohort {suffix}",
                TermStart = today.AddDays(-30),
                TermEnd = today.AddDays(30),
            };
            var session = new Session
            {
                CohortId = cohort.Id,
                VolunteerId = volunteer.Id,
                Title = $"Community Workshop {suffix}",
                StartsAt = today.AddDays(body.DaysFromToday).AddHours(18),
                DurationMinutes = 90,
                Location = "Main Hall",
                CreatedByUserId = volunteer.Id,
            };
            var youth = new[]
            {
                new User
                {
                    Email = $"amara.{suffix}@waypoint.test",
                    DisplayName = $"Amara {suffix}",
                    Role = Roles.Youth,
                    Status = UserStatus.Active,
                    CohortId = cohort.Id,
                },
                new User
                {
                    Email = $"david.{suffix}@waypoint.test",
                    DisplayName = $"David {suffix}",
                    Role = Roles.Youth,
                    Status = UserStatus.Active,
                    CohortId = cohort.Id,
                },
                new User
                {
                    Email = $"imani.{suffix}@waypoint.test",
                    DisplayName = $"Imani {suffix}",
                    Role = Roles.Youth,
                    Status = UserStatus.Active,
                    CohortId = cohort.Id,
                },
            };

            db.Cohorts.Add(cohort);
            db.Sessions.Add(session);
            db.Users.AddRange(youth);
            await db.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/volunteer/sessions/{session.Id}", new
            {
                sessionId = session.Id,
                youth = youth.Select(user => new { youthUserId = user.Id, name = user.DisplayName }),
            });
        });

        // Guardian E2E requires a linked child with aggregate-only progress inputs. Keep this rich fixture
        // behind Testing so production relationships still come only from validated intake/enrolment flows.
        app.MapPost("/api/_test/guardian-families", async (
            Waypoint.Api.Contracts.TestSupport.SeedGuardianFamilyBody body,
            IAppDbContext db,
            IClock clock,
            IPasswordHasher hasher,
            CancellationToken cancellationToken) =>
        {
            var guardian = await db.Users.FirstOrDefaultAsync(
                user => user.Id == body.GuardianUserId && user.Role == Roles.Guardian,
                cancellationToken);
            if (guardian is null)
            {
                return Results.NotFound(new { title = "Guardian not found." });
            }

            var suffix = Guid.NewGuid().ToString("N")[..8];
            var now = clock.UtcNow;
            var moduleName = $"Foundations {suffix}";
            var earnedBadgeName = $"First Steps {suffix}";
            var lockedBadgeName = $"Community Builder {suffix}";
            var upcomingTitle = $"Family Showcase {suffix}";
            var sharedActionItems = $"Prepare the welcome reflection {suffix}";
            var privateSecret = $"PRIVATE CHECK-IN {suffix}";
            var approvedQuizTitle = $"{moduleName} quiz";
            var competencyName = $"Listening well {suffix}";
            const string childPassword = "Password123";

            var track = new Track
            {
                Name = $"Guardian E2E Track {suffix}",
                Key = $"guardian-e2e-{suffix}",
                Status = PublishStatus.Published,
            };
            var module = new Module
            {
                TrackId = track.Id,
                Name = moduleName,
                Order = 1,
                Status = PublishStatus.Published,
            };
            var firstLesson = new Lesson
            {
                ModuleId = module.Id,
                Title = $"Servant Leadership {suffix}",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var secondLesson = new Lesson
            {
                ModuleId = module.Id,
                Title = $"Community Practice {suffix}",
                Order = 2,
                Status = PublishStatus.Published,
            };
            var cohort = new Cohort
            {
                Name = $"Guardian E2E Cohort {suffix}",
                TrackId = track.Id,
                TermStart = now.Date.AddDays(-30),
                TermEnd = now.Date.AddDays(90),
            };
            var child = new User
            {
                Email = $"guardian-child-{suffix}@waypoint.test",
                DisplayName = $"Jordan {suffix}",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                GuardianUserId = guardian.Id,
                CohortId = cohort.Id,
                OnboardingCompleted = true,
                PasswordHash = hasher.Hash(childPassword),
            };
            var presentSession = new Session
            {
                CohortId = cohort.Id,
                Title = $"Leadership Lab {suffix}",
                StartsAt = now.AddDays(-14),
                Location = "Studio A",
                CreatedByUserId = guardian.Id,
            };
            var absentSession = new Session
            {
                CohortId = cohort.Id,
                Title = $"Community Lab {suffix}",
                StartsAt = now.AddDays(-7),
                Location = "Studio B",
                CreatedByUserId = guardian.Id,
            };
            var upcomingSession = new Session
            {
                CohortId = cohort.Id,
                Title = upcomingTitle,
                StartsAt = now.AddDays(7),
                Location = "Main Hall",
                CreatedByUserId = guardian.Id,
            };
            var earnedBadge = new Badge
            {
                Name = earnedBadgeName,
                Icon = "star",
                Description = "Started the journey.",
            };
            var lockedBadge = new Badge
            {
                Name = lockedBadgeName,
                Icon = "award",
                Description = "Complete the community module.",
            };
            var quiz = new Quiz
            {
                ModuleId = module.Id,
                PassMark = 70,
            };
            var progressionStage = new ProgressionStage
            {
                Name = $"Leadership {suffix}",
                Order = 1,
            };
            var competency = new Competency
            {
                StageId = progressionStage.Id,
                Name = competencyName,
                Order = 1,
            };

            db.Tracks.Add(track);
            db.Modules.Add(module);
            db.Lessons.AddRange(firstLesson, secondLesson);
            db.Cohorts.Add(cohort);
            db.Users.Add(child);
            db.Sessions.AddRange(presentSession, absentSession, upcomingSession);
            db.LessonProgress.Add(new LessonProgress
            {
                YouthUserId = child.Id,
                LessonId = firstLesson.Id,
                Status = ProgressStatus.Completed,
                CompletedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-10),
            });
            db.AttendanceRecords.AddRange(
                new AttendanceRecord
                {
                    SessionId = presentSession.Id,
                    YouthUserId = child.Id,
                    Status = AttendanceStatus.Present,
                    RecordedByUserId = guardian.Id,
                    RecordedAt = presentSession.StartsAt,
                },
                new AttendanceRecord
                {
                    SessionId = absentSession.Id,
                    YouthUserId = child.Id,
                    Status = AttendanceStatus.Absent,
                    RecordedByUserId = guardian.Id,
                    RecordedAt = absentSession.StartsAt,
                });
            db.Badges.AddRange(earnedBadge, lockedBadge);
            db.Quizzes.Add(quiz);
            db.QuizAttempts.AddRange(
                new QuizAttempt
                {
                    YouthUserId = child.Id,
                    QuizId = quiz.Id,
                    AttemptNumber = 1,
                    IsSubmitted = true,
                    AutoScore = 80,
                    ApprovedScore = 80,
                    ApprovedByUserId = guardian.Id,
                    ApprovedAt = now.AddDays(-8),
                    SubmittedAt = now.AddDays(-9),
                    AnswersJson = privateSecret,
                },
                new QuizAttempt
                {
                    YouthUserId = child.Id,
                    QuizId = quiz.Id,
                    AttemptNumber = 2,
                    IsSubmitted = true,
                    AutoScore = 100,
                    SubmittedAt = now.AddDays(-1),
                    AnswersJson = privateSecret,
                });
            db.ProgressionStages.Add(progressionStage);
            db.Competencies.Add(competency);
            db.CompetencyAssessments.AddRange(
                new CompetencyAssessment
                {
                    YouthUserId = child.Id,
                    CompetencyId = competency.Id,
                    Level = CompetencyLevel.Developing,
                    AssessedByUserId = guardian.Id,
                    AssessedAt = now.AddDays(-12),
                },
                new CompetencyAssessment
                {
                    YouthUserId = child.Id,
                    CompetencyId = competency.Id,
                    Level = CompetencyLevel.Proficient,
                    Notes = privateSecret,
                    AssessedByUserId = guardian.Id,
                    AssessedAt = now.AddDays(-4),
                });
            db.BadgeAwards.Add(new BadgeAward
            {
                BadgeId = earnedBadge.Id,
                YouthUserId = child.Id,
                AwardedAt = now.AddDays(-9),
            });
            db.CheckIns.AddRange(
                new CheckIn
                {
                    YouthUserId = child.Id,
                    MentorUserId = guardian.Id,
                    Date = now.AddDays(-3),
                    Type = "1:1",
                    Engagement = "Engaged and contributing",
                    GoalsDiscussed = "Welcome new participants",
                    PrayerRequests = "Confidence for the showcase",
                    ActionItems = sharedActionItems,
                    NextFollowUp = now.AddDays(4),
                    Visibility = Visibility.SharedWithMentor,
                },
                new CheckIn
                {
                    YouthUserId = child.Id,
                    MentorUserId = guardian.Id,
                    Date = now.AddDays(-2),
                    Type = "call",
                    Engagement = privateSecret,
                    PrayerRequests = privateSecret,
                    Visibility = Visibility.Private,
                });
            db.GuardianProfiles.Add(new GuardianProfile
            {
                GuardianUserId = guardian.Id,
                Relationship = "Parent",
                Phone = "+14165550100",
                EmergencyContactName = "Taylor Family",
                EmergencyContactPhone = "+14165550111",
                UpdatedAt = now,
            });
            if (body.IncludeConsent)
            {
                db.GuardianConsents.Add(new GuardianConsent
                {
                    YouthUserId = child.Id,
                    GuardianUserId = guardian.Id,
                    GuardianName = guardian.DisplayName,
                    Relationship = "Parent",
                    ParticipationConsent = true,
                    Signature = guardian.DisplayName,
                    ReferenceId = $"CONSENT-{suffix.ToUpperInvariant()}",
                    Term = "Current term",
                    SignedAt = now.AddDays(-20),
                    ValidFrom = now.AddDays(-20),
                    ValidUntil = now.AddDays(90),
                });
            }
            await db.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/guardian/children/{child.Id}", new
            {
                childId = child.Id,
                cohortId = cohort.Id,
                childName = child.DisplayName,
                childEmail = child.Email,
                childPassword,
                moduleName,
                earnedBadgeName,
                lockedBadgeName,
                upcomingTitle,
                sharedActionItems,
                privateSecret,
                approvedQuizTitle,
                competencyName,
            });
        });

        app.MapGet("/api/_test/emails/latest", (
            string to,
            Waypoint.Api.TestSupport.TestingEmailSender emails) =>
        {
            var email = emails.LastTo(to);
            return email is null
                ? Results.NotFound(new { title = "No matching email has been captured." })
                : Results.Ok(new { email.To, email.Subject, email.Body });
        });

        // Rich youth curriculum fixture: two explicit enrollments, ordered lesson/quiz gating, a locked
        // prerequisite module, and an empty secondary track. Production enrollment remains admin-owned.
        app.MapPost("/api/_test/youth-curriculum", async (
            IAppDbContext db,
            IPasswordHasher hasher,
            IClock clock,
            CancellationToken cancellationToken) =>
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var now = clock.UtcNow;
            const string youthPassword = "Password123";
            var primaryTrackName = $"Bible Foundations {suffix}";
            var secondTrackName = $"Leadership Practice {suffix}";
            var firstModuleName = $"Foundations {suffix}";
            var lockedModuleName = $"Deeper Practice {suffix}";
            var firstLessonTitle = $"Build a welcome card {suffix}";
            var secondLessonTitle = $"Living the Word {suffix}";
            var leaderSecret = $"MENTOR-ONLY GUIDE {suffix}";

            var guardian = new User
            {
                Email = $"curriculum-guardian-{suffix}@waypoint.test",
                DisplayName = $"Curriculum Guardian {suffix}",
                Role = Roles.Guardian,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(youthPassword),
                OnboardingCompleted = true,
            };
            var primaryTrack = new Track
            {
                Name = primaryTrackName,
                Key = $"curriculum-primary-{suffix}",
                Description = "The primary enrolled learning track.",
                Status = PublishStatus.Published,
            };
            var secondTrack = new Track
            {
                Name = secondTrackName,
                Key = $"curriculum-secondary-{suffix}",
                Description = "A second enrolled track awaiting content.",
                Status = PublishStatus.Published,
            };
            var firstModule = new Module
            {
                TrackId = primaryTrack.Id,
                Name = firstModuleName,
                Description = "Begin with scripture and everyday practice.",
                ScriptureAnchor = "Mark 10:45",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var lockedModule = new Module
            {
                TrackId = primaryTrack.Id,
                Name = lockedModuleName,
                Description = "Build on the foundations module.",
                Order = 2,
                Status = PublishStatus.Published,
                PrerequisiteModuleId = firstModule.Id,
            };
            var draftModule = new Module
            {
                TrackId = primaryTrack.Id,
                Name = "Draft module",
                Order = 99,
                Status = PublishStatus.Draft,
            };
            var firstLesson = new Lesson
            {
                ModuleId = firstModule.Id,
                Title = firstLessonTitle,
                TeachingBody = "Follow the steps, edit the HTML, and check your work in the live preview.",
                StarterCode = "<h1>Hello!</h1>",
                IsCodingLesson = true,
                EstimatedMinutes = 12,
                Order = 1,
                Status = PublishStatus.Published,
            };
            var secondLesson = new Lesson
            {
                ModuleId = firstModule.Id,
                Title = secondLessonTitle,
                ScriptureReference = "Luke 10:27",
                LearningObjectives = "Put love into practice in everyday relationships.",
                TeachingBody = "Jesus calls us to put love into practice with our whole lives.",
                ReflectionPrompts = "Where can you practise neighbour-love this week?",
                LeaderGuideNotes = leaderSecret,
                EstimatedMinutes = 15,
                Order = 2,
                Status = PublishStatus.Published,
            };
            var quiz = new Quiz
            {
                ModuleId = firstModule.Id,
                PassMark = 70,
                AttemptsAllowed = 2,
                RequiredToCompleteModule = true,
            };
            var progressQuiz = new Quiz
            {
                LessonId = secondLesson.Id,
                PassMark = 70,
                AttemptsAllowed = 2,
            };
            var firstQuizQuestion = new QuizQuestion
            {
                QuizId = quiz.Id,
                Text = "What does Jesus say is the greatest commandment?",
                Type = QuestionType.MultipleChoice,
                Order = 1,
                OptionsJson = JsonSerializer.Serialize(new[]
                {
                    "Love God with your whole life",
                    "Look after only yourself",
                }),
                CorrectAnswersJson = JsonSerializer.Serialize(new[] { 0 }),
            };
            var secondQuizQuestion = new QuizQuestion
            {
                QuizId = quiz.Id,
                Text = "Who are we called to love as ourselves?",
                Type = QuestionType.MultipleChoice,
                Order = 2,
                OptionsJson = JsonSerializer.Serialize(new[]
                {
                    "Only people we already know",
                    "Our neighbour",
                }),
                CorrectAnswersJson = JsonSerializer.Serialize(new[] { 1 }),
            };
            var mentorQuizQuestion = new QuizQuestion
            {
                QuizId = progressQuiz.Id,
                Text = "Which habit helps build a welcoming community?",
                Type = QuestionType.MultipleChoice,
                Order = 1,
                OptionsJson = JsonSerializer.Serialize(new[]
                {
                    "Listen carefully and offer help",
                    "Ignore people who are new",
                }),
                CorrectAnswersJson = JsonSerializer.Serialize(new[] { 0 }),
            };
            var quizBadge = new Badge
            {
                Name = $"Neighbour Love Badge {suffix}",
                Icon = "heart",
                Description = "Pass the foundations module quiz.",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "quiz-passed",
                TargetRefId = quiz.Id,
            };
            var codingFirstStep = new CodingLessonStep
            {
                LessonId = firstLesson.Id,
                Order = 1,
                Title = "Add a welcome heading",
                Instructions = "Change the h1 so it welcomes people to your page.",
                CodeSample = "<h1>Welcome to my page</h1>",
                ChecklistLabel = "My page has a welcome heading",
            };
            var codingSecondStep = new CodingLessonStep
            {
                LessonId = firstLesson.Id,
                Order = 2,
                Title = "Add an introduction",
                Instructions = "Add a paragraph that tells visitors something about you.",
                CodeSample = "<p>I am learning to build for the web.</p>",
                ChecklistLabel = "My page has an introduction paragraph",
            };
            var codingAssignment = new Assignment
            {
                ModuleId = firstModule.Id,
                LessonId = firstLesson.Id,
                Title = $"Welcome Card Coding Project {suffix}",
                TaskDescription = "Submit the welcome card from your coding workspace.",
                SubmissionTypes = "text",
                RequireMentorApproval = true,
                TotalPoints = 10,
            };
            var codingBadge = new Badge
            {
                Name = $"Code Starter Badge {suffix}",
                Icon = "code",
                Description = "Complete every self-check in the welcome card lesson.",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "coding-checklist-completed",
                TargetRefId = firstLesson.Id,
            };
            var journeyBadge = new Badge
            {
                Name = $"First Steps Badge {suffix}",
                Icon = "footprints",
                Description = "Complete your first lesson.",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "lesson-completed",
                TargetRefId = firstLesson.Id,
            };
            var assignment = new Assignment
            {
                ModuleId = firstModule.Id,
                Title = $"Neighbour Love Project {suffix}",
                TaskDescription = "Describe one practical act of neighbour-love and include evidence if you can.",
                SubmissionTypes = "text,photo,link,file",
                DueDate = now.AddDays(7),
                AllowLate = true,
                RequireMentorApproval = true,
                TotalPoints = 10,
            };
            var mentorReviewAssignment = new Assignment
            {
                ModuleId = firstModule.Id,
                Title = $"Service Reflection Review {suffix}",
                TaskDescription = "Review one way you served someone this week.",
                SubmissionTypes = "text",
                DueDate = now.AddDays(-2),
                AllowLate = true,
                RequireMentorApproval = true,
                TotalPoints = 10,
            };
            var mentorReviewCriterion = new RubricCriterion
            {
                AssignmentId = mentorReviewAssignment.Id,
                Description = "Connects service to leadership practice",
                Points = 10,
                Order = 1,
            };
            var assignmentCriterion = new RubricCriterion
            {
                AssignmentId = assignment.Id,
                Description = "Connects scripture to a practical action",
                Points = 10,
                Order = 1,
            };
            var assignmentBadge = new Badge
            {
                Name = $"Practical Love Badge {suffix}",
                Icon = "hands-heart",
                Description = "Earned when the neighbour-love project is approved.",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "assignment-approved",
                TargetRefId = assignment.Id,
            };
            var mentor = new User
            {
                Email = $"curriculum-mentor-{suffix}@waypoint.test",
                DisplayName = $"Curriculum Mentor {suffix}",
                Role = Roles.Mentor,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(youthPassword),
                OnboardingCompleted = true,
            };
            var cohort = new Cohort
            {
                Name = $"Curriculum Cohort {suffix}",
                TrackId = primaryTrack.Id,
                TermStart = now.Date.AddDays(-30),
                TermEnd = now.Date.AddDays(120),
                MentorId = mentor.Id,
            };
            var youth = new User
            {
                Email = $"curriculum-youth-{suffix}@waypoint.test",
                DisplayName = $"Curriculum Youth {suffix}",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(youthPassword),
                OnboardingCompleted = true,
                CohortId = cohort.Id,
                GuardianUserId = guardian.Id,
                MentorId = mentor.Id,
            };
            var progressStage = new ProgressionStage
            {
                Name = $"Leadership Growth {suffix}",
                Order = 1,
            };
            var progressCompetency = new Competency
            {
                StageId = progressStage.Id,
                Name = "Serving others",
                Order = 1,
            };
            var mentorCompetency = new Competency
            {
                StageId = progressStage.Id,
                Name = "Community initiative",
                Order = 2,
            };
            var presentSession = new Session
            {
                CohortId = cohort.Id,
                MentorId = mentor.Id,
                Title = $"Leadership Lab {suffix}",
                StartsAt = now.AddDays(-7),
                DurationMinutes = 90,
                CreatedByUserId = mentor.Id,
            };
            var upcomingSession = new Session
            {
                CohortId = cohort.Id,
                MentorId = mentor.Id,
                Title = $"Community Project Workshop {suffix}",
                StartsAt = now.AddDays(4),
                DurationMinutes = 75,
                Location = "Waypoint Community Hub",
                CreatedByUserId = mentor.Id,
            };
            var absentSession = new Session
            {
                CohortId = cohort.Id,
                MentorId = mentor.Id,
                Title = $"Community Night {suffix}",
                StartsAt = now.AddDays(-14),
                DurationMinutes = 90,
                CreatedByUserId = mentor.Id,
            };

            db.Users.AddRange(guardian, mentor, youth);
            db.GuardianProfiles.Add(new GuardianProfile
            {
                GuardianUserId = guardian.Id,
                Relationship = "Parent",
                Phone = "416-555-0142",
                EmergencyContactName = "Community Auntie",
                EmergencyContactPhone = "416-555-0188",
                UpdatedAt = now,
            });
            db.Tracks.AddRange(primaryTrack, secondTrack);
            db.Modules.AddRange(firstModule, lockedModule, draftModule);
            db.Lessons.AddRange(firstLesson, secondLesson);
            db.CodingLessonSteps.AddRange(codingFirstStep, codingSecondStep);
            db.LessonMedia.Add(new LessonMedia
            {
                LessonId = secondLesson.Id,
                MediaType = "video",
                Url = "https://media.example.test/lesson-video.mp4",
                CreatedAt = now,
            });
            db.Quizzes.AddRange(quiz, progressQuiz);
            db.QuizQuestions.AddRange(firstQuizQuestion, secondQuizQuestion, mentorQuizQuestion);
            db.Badges.AddRange(quizBadge, codingBadge, journeyBadge);
            db.BadgeAwards.Add(new BadgeAward
            {
                BadgeId = journeyBadge.Id,
                YouthUserId = youth.Id,
                AwardedAt = now.AddDays(-1),
            });
            db.Assignments.AddRange(assignment, codingAssignment, mentorReviewAssignment);
            db.RubricCriteria.AddRange(assignmentCriterion, mentorReviewCriterion);
            db.Badges.Add(assignmentBadge);
            db.Cohorts.Add(cohort);
            db.ProgressionStages.Add(progressStage);
            db.Competencies.AddRange(progressCompetency, mentorCompetency);
            db.CompetencyAssessments.Add(new CompetencyAssessment
            {
                YouthUserId = youth.Id,
                CompetencyId = progressCompetency.Id,
                Level = CompetencyLevel.Proficient,
                AssessedByUserId = mentor.Id,
                AssessedAt = now.AddDays(-2),
            });
            db.QuizAttempts.Add(new QuizAttempt
            {
                YouthUserId = youth.Id,
                QuizId = progressQuiz.Id,
                AttemptNumber = 1,
                AutoScore = 82,
                Passed = true,
                IsSubmitted = true,
                SubmittedAt = now.AddDays(-3),
                ApprovedScore = 82,
                ApprovedByUserId = mentor.Id,
                ApprovedAt = now.AddDays(-2),
            });
            var mentorQuizAttempt = new QuizAttempt
            {
                YouthUserId = youth.Id,
                QuizId = progressQuiz.Id,
                AttemptNumber = 2,
                AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, int[]>
                {
                    [mentorQuizQuestion.Id] = [1],
                }),
                AutoScore = 40,
                Passed = false,
                IsSubmitted = true,
                SubmittedAt = now.AddHours(-6),
            };
            db.QuizAttempts.Add(mentorQuizAttempt);
            db.Sessions.AddRange(presentSession, absentSession, upcomingSession);
            db.Messages.Add(new Message
            {
                ThreadKey = Message.KeyFor(mentor.Id, youth.Id),
                SenderId = mentor.Id,
                RecipientId = youth.Id,
                Body = "Welcome! Message me whenever you need support.",
                SentAt = now.AddMinutes(-30),
            });
            db.CheckIns.Add(new CheckIn
            {
                YouthUserId = youth.Id,
                MentorUserId = mentor.Id,
                Date = now.AddDays(-2),
                Type = "1:1",
                Engagement = "Engaged",
                Visibility = Visibility.Private,
                PrayerRequests = "PRIVATE PRAYER DETAIL",
                NextFollowUp = now.AddDays(5),
                CreatedAt = now.AddDays(-2),
            });
            db.AssignedWork.Add(new AssignedWork
            {
                ItemType = "Project",
                ItemId = assignment.Id,
                YouthUserId = youth.Id,
                DueDate = now.AddDays(5),
                Message = "Bring your leadership ideas to this project.",
                AssignedByUserId = mentor.Id,
                CreatedAt = now.AddDays(-1),
            });
            var mentorReviewSubmission = new Submission
            {
                YouthUserId = youth.Id,
                AssignmentId = mentorReviewAssignment.Id,
                BodyText = "I helped welcome a new person and made sure they had someone to talk to.",
                Status = SubmissionStatus.Submitted,
                SubmittedAt = now.AddDays(-1),
                CreatedAt = now.AddDays(-1),
            };
            db.Submissions.Add(mentorReviewSubmission);
            db.Flags.Add(new Flag
            {
                YouthUserId = youth.Id,
                Reason = "Follow up after two missed sessions.",
                FlaggedByUserId = mentor.Id,
                FlaggedAt = now.AddHours(-4),
            });
            db.AttendanceRecords.AddRange(
                new AttendanceRecord
                {
                    SessionId = presentSession.Id,
                    YouthUserId = youth.Id,
                    Status = AttendanceStatus.Present,
                    RecordedByUserId = mentor.Id,
                    RecordedAt = now.AddDays(-7),
                },
                new AttendanceRecord
                {
                    SessionId = absentSession.Id,
                    YouthUserId = youth.Id,
                    Status = AttendanceStatus.Absent,
                    RecordedByUserId = mentor.Id,
                    RecordedAt = now.AddDays(-14),
                });
            db.YouthTrackEnrollments.Add(new YouthTrackEnrollment
            {
                YouthUserId = youth.Id,
                TrackId = secondTrack.Id,
                EnrolledAt = now,
            });
            db.LessonProgress.Add(new LessonProgress
            {
                YouthUserId = youth.Id,
                LessonId = firstLesson.Id,
                Status = ProgressStatus.Completed,
                CompletedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1),
            });
            db.GuardianConsents.Add(new GuardianConsent
            {
                YouthUserId = youth.Id,
                GuardianUserId = guardian.Id,
                GuardianName = guardian.DisplayName,
                Relationship = "Parent",
                ParticipationConsent = true,
                MediaConsent = true,
                MediaAllPhotosVideo = true,
                Signature = guardian.DisplayName,
                ReferenceId = $"CURRIC-{suffix.ToUpperInvariant()}",
                SignedAt = now.AddDays(-5),
                ValidFrom = now.AddDays(-5),
                ValidUntil = now.AddMonths(6),
            });
            await db.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/me/curriculum/{primaryTrack.Id}", new
            {
                youthId = youth.Id,
                youthName = youth.DisplayName,
                guardianName = guardian.DisplayName,
                guardianEmail = guardian.Email,
                youthEmail = youth.Email,
                youthPassword,
                primaryTrackId = primaryTrack.Id,
                primaryTrackName,
                secondTrackId = secondTrack.Id,
                secondTrackName,
                firstModuleName,
                lockedModuleName,
                firstLessonTitle,
                firstLessonId = firstLesson.Id,
                secondLessonTitle,
                secondLessonId = secondLesson.Id,
                leaderSecret,
                quizId = quiz.Id,
                quizTitle = $"{firstModuleName} quiz",
                firstQuizQuestion = firstQuizQuestion.Text,
                secondQuizQuestion = secondQuizQuestion.Text,
                firstCorrectAnswer = "Love God with your whole life",
                secondCorrectAnswer = "Our neighbour",
                quizBadgeName = quizBadge.Name,
                mentorEmail = mentor.Email,
                mentorPassword = youthPassword,
                mentorName = mentor.DisplayName,
                mentorId = mentor.Id,
                cohortId = cohort.Id,
                cohortName = cohort.Name,
                assignmentId = assignment.Id,
                assignmentTitle = assignment.Title,
                assignmentCriterionId = assignmentCriterion.Id,
                assignmentCriterion = assignmentCriterion.Description,
                assignmentBadgeName = assignmentBadge.Name,
                codingLessonId = firstLesson.Id,
                codingLessonTitle = firstLesson.Title,
                codingFirstStepTitle = codingFirstStep.Title,
                codingSecondStepTitle = codingSecondStep.Title,
                codingBadgeName = codingBadge.Name,
                journeyBadgeName = journeyBadge.Name,
                progressQuizTitle = $"{secondLesson.Title} quiz",
                progressPresentSessionTitle = presentSession.Title,
                progressAbsentSessionTitle = absentSession.Title,
                progressCompetencyName = progressCompetency.Name,
                mentorCompetencyId = mentorCompetency.Id,
                mentorCompetencyName = mentorCompetency.Name,
                scheduleSessionId = upcomingSession.Id,
                scheduleSessionTitle = upcomingSession.Title,
                scheduleSessionLocation = upcomingSession.Location,
                mentorReviewSubmissionId = mentorReviewSubmission.Id,
                mentorReviewTitle = mentorReviewAssignment.Title,
                mentorReviewCriterionId = mentorReviewCriterion.Id,
                mentorReviewCriterion = mentorReviewCriterion.Description,
                mentorQuizAttemptId = mentorQuizAttempt.Id,
                mentorQuizTitle = $"{secondLesson.Title} quiz",
                mentorQuizAutoScore = mentorQuizAttempt.AutoScore,
                mentorQuizQuestion = mentorQuizQuestion.Text,
                mentorQuizSelectedAnswer = "Ignore people who are new",
            });
        });

        // Public-content E2E needs published curriculum and a privacy-safe cohort. Product APIs exist
        // for each step (track publish, badge targets), but this seeds them in one call instead of a
        // long authenticated sequence. Keep this seed behind Testing only.
        app.MapPost("/api/_test/public-content", async (
            IAppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var track = new Track
            {
                Name = $"E2E Leadership Track {suffix}",
                Key = $"e2e-leadership-{suffix}",
                Description = "Leadership content served by the authoritative API.",
                Status = PublishStatus.Published,
                Enabled = true,
            };
            var module = new Module
            {
                TrackId = track.Id,
                Name = $"E2E Servant Leadership Module {suffix}",
                Description = "Lead by serving.",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var badge = new Badge
            {
                Name = $"E2E Servant Heart Badge {suffix}",
                Icon = "heart",
                Description = "Complete the module.",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "module-completed",
                TargetRefId = module.Id,
            };
            var mentor = new User
            {
                Email = $"e2e-public-mentor-{suffix}@waypoint.test",
                DisplayName = $"E2E Pat Mentor {suffix}",
                Role = Roles.Mentor,
                Status = UserStatus.Active,
            };
            var cohort = new Cohort
            {
                Name = $"E2E Regent Park Cohort {suffix}",
                TrackId = track.Id,
                Location = "Regent Park",
                TermStart = new DateTime(2999, 9, 8, 0, 0, 0, DateTimeKind.Utc),
                TermEnd = new DateTime(2999, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                MentorId = mentor.Id,
            };

            db.Tracks.Add(track);
            db.Modules.Add(module);
            db.Badges.Add(badge);
            db.Users.Add(mentor);
            db.Cohorts.Add(cohort);
            await db.SaveChangesAsync(cancellationToken);

            return Results.Created("/api/public/content", new
            {
                trackName = track.Name,
                moduleName = module.Name,
                badgeName = badge.Name,
                cohortName = cohort.Name,
                neighbourhood = cohort.Location,
                mentorName = mentor.DisplayName,
                cohort.TermStart,
                cohort.TermEnd,
            });
        });
    }
}

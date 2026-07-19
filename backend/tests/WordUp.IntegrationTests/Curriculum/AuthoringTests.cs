using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Curriculum;

// Acceptance Test
// Traces to: L2-CURRIC-001 (tracks/modules authoring, ordering, publish),
//            L2-CURRIC-002 (lessons authoring, staff view, soft-delete)
[TestFixture]
[Property("Req", "L2-CURRIC-001")]
public sealed class AuthoringTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string? accessToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        if (accessToken is not null)
        {
            Authorize(request, accessToken);
        }
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, accessToken);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string url, object body, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        Authorize(request, accessToken);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> DeleteAsync(string url, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        Authorize(request, accessToken);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> UploadAsync(
        string url,
        string fileName,
        string contentType,
        byte[] content,
        string accessToken)
    {
        using var multipart = new MultipartFormDataContent();
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        multipart.Add(file, "file", fileName);
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = multipart };
        Authorize(request, accessToken);
        return await Client.SendAsync(request);
    }

    // Traces to: L2-CURRIC-001 (AC1 unique key, authorization)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    public async Task Admin_creates_a_track_duplicate_key_conflicts_and_youth_is_forbidden()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var key = $"bible-{Guid.NewGuid():N}";

        var created = await PostAsync("/api/tracks", new { name = "Bible Foundations", key, description = "OT/NT" }, adminToken);
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await created.Content.ReadFromJsonAsync<TrackResultDto>();
        Assert.That(result!.Id, Is.Not.EqualTo(Guid.Empty));

        // AC1: a second track with the same key is rejected.
        var duplicate = await PostAsync("/api/tracks", new { name = "Bible Again", key }, adminToken);
        Assert.That(duplicate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        // Authorization: a youth cannot author curriculum.
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await PostAsync("/api/tracks", new { name = "Nope", key = $"x-{Guid.NewGuid():N}" }, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    // Traces to: L2-CURRIC-001 (AC2 create modules, AC3 reorder persists)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    public async Task Modules_can_be_created_and_reordered_and_the_new_order_persists()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks", new { name = "Leadership", key = $"lead-{Guid.NewGuid():N}" }, adminToken))
            .Content.ReadFromJsonAsync<TrackResultDto>())!.Id;

        var first = (await (await PostAsync("/api/modules", new { trackId, name = "Module A" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var second = (await (await PostAsync("/api/modules", new { trackId, name = "Module B" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;

        // Reverse the order.
        var reorder = await PostAsync(
            "/api/modules/reorder", new { trackId, orderedModuleIds = new[] { second, first } }, adminToken);
        Assert.That(reorder.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // AC3: the new order is persisted.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var firstModule = await db.Modules.FirstAsync(m => m.Id == first);
        var secondModule = await db.Modules.FirstAsync(m => m.Id == second);
        Assert.That(secondModule.Order, Is.EqualTo(1));
        Assert.That(firstModule.Order, Is.EqualTo(2));
    }

    // Traces to: L2-CURRIC-001 (AC4 publish a module)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    public async Task Publishing_a_module_sets_its_status_to_published()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks", new { name = "Coding", key = $"code-{Guid.NewGuid():N}" }, adminToken))
            .Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync("/api/modules", new { trackId, name = "Intro" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;

        var publish = await PostAsync(
            $"/api/modules/{moduleId}/publish", new { status = (int)PublishStatus.Published }, adminToken);
        Assert.That(publish.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = await db.Modules.FirstAsync(m => m.Id == moduleId);
        Assert.That(module.Status, Is.EqualTo(PublishStatus.Published));
    }

    // Traces to: L2-CURRIC-002 (AC1 lesson persists learning objectives + leader guide + teaching body)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Creating_a_lesson_persists_all_authoring_fields_and_the_staff_view_returns_them()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks", new { name = "Discipleship", key = $"disc-{Guid.NewGuid():N}" }, adminToken))
            .Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync("/api/modules", new { trackId, name = "Week 1" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;

        var lessonId = (await (await PostAsync("/api/lessons", new
        {
            moduleId,
            title = "The Sermon on the Mount",
            scriptureReference = "Matthew 5-7",
            estimatedMinutes = 45,
            learningObjectives = "Understand the Beatitudes.",
            teachingBody = "The core teaching narrative.",
            reflectionPrompts = "What does mercy mean to you?",
            leaderGuideNotes = "Encourage quiet reflection first.",
        }, adminToken)).Content.ReadFromJsonAsync<LessonResultDto>())!.Id;

        var get = await GetAsync($"/api/lessons/{lessonId}", adminToken);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var detail = await get.Content.ReadFromJsonAsync<LessonDetailDto>();
        Assert.That(detail!.LearningObjectives, Is.EqualTo("Understand the Beatitudes."));
        Assert.That(detail.TeachingBody, Is.EqualTo("The core teaching narrative."));
        Assert.That(detail.LeaderGuideNotes, Is.EqualTo("Encourage quiet reflection first."));
        Assert.That(detail.ReflectionPrompts, Is.EqualTo("What does mercy mean to you?"));
        Assert.That(detail.EstimatedMinutes, Is.EqualTo(45));
    }

    // Traces to: L2-CURRIC-002 (AC5 soft-delete: staff view 404 but row retained)
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Deleting_a_lesson_soft_archives_it_so_the_view_is_gone_but_the_row_remains()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks", new { name = "Service", key = $"serv-{Guid.NewGuid():N}" }, adminToken))
            .Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync("/api/modules", new { trackId, name = "Outreach" }, adminToken))
            .Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var lessonId = (await (await PostAsync(
            "/api/lessons", new { moduleId, title = "Serving Others" }, adminToken))
            .Content.ReadFromJsonAsync<LessonResultDto>())!.Id;

        var delete = await DeleteAsync($"/api/lessons/{lessonId}", adminToken);
        Assert.That(delete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // AC5: the staff view treats an archived lesson as gone.
        var get = await GetAsync($"/api/lessons/{lessonId}", adminToken);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        // AC5: the row is retained with ArchivedAt set (progress/linked quizzes are preserved).
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var lesson = await db.Lessons.FirstAsync(l => l.Id == lessonId);
        Assert.That(lesson.ArchivedAt, Is.Not.Null);
    }

    // Traces to: L2-CURRIC-001 (AC2/AC4 complete ordered workspace + cohort impact),
    //            L2-CURRIC-002 (AC1 mentor-only authoring content)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Admin_reads_the_complete_ordered_track_workspace_and_mentor_is_forbidden()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks",
            new
            {
                name = "Servant Leadership",
                key = $"workspace-{Guid.NewGuid():N}",
                description = "A practical leadership pathway.",
            },
            adminToken)).Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var foundationId = (await (await PostAsync(
            "/api/modules",
            new
            {
                trackId,
                name = "Foundations",
                description = "Lead by serving.",
                scriptureAnchor = "Mark 10:45",
            },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var practiceId = (await (await PostAsync(
            "/api/modules",
            new
            {
                trackId,
                name = "Practice",
                description = "Put service into action.",
                prerequisiteModuleId = foundationId,
                scriptureAnchor = "James 2:17",
            },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var lessonId = (await (await PostAsync(
            "/api/lessons",
            new
            {
                moduleId = foundationId,
                title = "The posture of a servant",
                scriptureReference = "Philippians 2:3-8",
                estimatedMinutes = 35,
                learningObjectives = "Describe servant leadership.",
                teachingBody = "A complete teaching body.",
                reflectionPrompts = "Who can you serve this week?",
                leaderGuideNotes = "Invite examples without naming vulnerable people.",
            },
            adminToken)).Content.ReadFromJsonAsync<LessonResultDto>())!.Id;

        Guid quizId;
        Guid assignmentId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var quiz = new Quiz
            {
                ModuleId = foundationId,
                PassMark = 75,
                AttemptsAllowed = 3,
                RequiredToCompleteModule = true,
            };
            var question = new QuizQuestion
            {
                QuizId = quiz.Id,
                Text = "What posture did Jesus model?",
                Type = QuestionType.ShortAnswer,
                Order = 1,
                CorrectAnswersJson = "[\"service\"]",
            };
            var assignment = new Assignment
            {
                LessonId = lessonId,
                Title = "Service reflection",
                TaskDescription = "Describe one act of service.",
                SubmissionTypes = "text",
            };
            db.Quizzes.Add(quiz);
            db.QuizQuestions.Add(question);
            db.Assignments.Add(assignment);
            db.RubricCriteria.Add(new RubricCriterion
            {
                AssignmentId = assignment.Id,
                Description = "Specific reflection",
                Points = 5,
                Order = 1,
            });
            db.Cohorts.Add(new Cohort
            {
                Name = "Thursday Leaders",
                TrackId = trackId,
                TermStart = DateTime.UtcNow.Date,
                TermEnd = DateTime.UtcNow.Date.AddMonths(3),
            });
            await db.SaveChangesAsync();
            quizId = quiz.Id;
            assignmentId = assignment.Id;
        }

        var response = await GetAsync($"/api/tracks/{trackId}", adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var workspace = await response.Content.ReadFromJsonAsync<TrackWorkspaceDto>();
        Assert.Multiple(() =>
        {
            Assert.That(workspace!.Description, Is.EqualTo("A practical leadership pathway."));
            Assert.That(workspace.Status, Is.EqualTo("Draft"));
            Assert.That(workspace.AssignedCohortNames, Is.EqualTo(new[] { "Thursday Leaders" }));
            Assert.That(workspace.Modules.Select(item => item.Id), Is.EqualTo(new[] { foundationId, practiceId }));
            Assert.That(workspace.Modules[1].PrerequisiteModuleId, Is.EqualTo(foundationId));
            Assert.That(workspace.Modules[1].ScriptureAnchor, Is.EqualTo("James 2:17"));
            Assert.That(workspace.Modules[0].Lessons.Single().Id, Is.EqualTo(lessonId));
            Assert.That(workspace.Modules[0].Lessons.Single().LeaderGuideNotes,
                Is.EqualTo("Invite examples without naming vulnerable people."));
            Assert.That(workspace.Modules[0].Quizzes.Single().Id, Is.EqualTo(quizId));
            Assert.That(workspace.Modules[0].Quizzes.Single().QuestionCount, Is.EqualTo(1));
            Assert.That(workspace.Modules[0].Lessons.Single().Assignments.Single().Id,
                Is.EqualTo(assignmentId));
            Assert.That(workspace.Modules[0].Lessons.Single().Assignments.Single().TotalPoints,
                Is.EqualTo(5));
        });

        var mentorToken = await CreateUserAndGetTokenAsync(Roles.Mentor);
        var forbidden = await GetAsync($"/api/tracks/{trackId}", mentorToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    // Traces to: L2-CURRIC-001 (AC1/AC4 track publish lifecycle drives youth visibility)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    public async Task Publishing_a_track_makes_its_published_content_visible_to_an_assigned_youth()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks",
            new { name = "Visible Track", key = $"visible-{Guid.NewGuid():N}" },
            adminToken)).Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync(
            "/api/modules",
            new { trackId, name = "Visible Module" },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var lessonId = (await (await PostAsync(
            "/api/lessons",
            new { moduleId, title = "Visible Lesson" },
            adminToken)).Content.ReadFromJsonAsync<LessonResultDto>())!.Id;
        Assert.That((await PostAsync(
            $"/api/modules/{moduleId}/publish",
            new { status = (int)PublishStatus.Published },
            adminToken)).StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await PostAsync(
            $"/api/lessons/{lessonId}/publish",
            new { status = (int)PublishStatus.Published },
            adminToken)).StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var email = $"published.youth.{Guid.NewGuid():N}@example.com";
        const string password = "Passw0rd!";
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var cohort = new Cohort
            {
                Name = "Published Track Cohort",
                TrackId = trackId,
                TermStart = DateTime.UtcNow.Date,
                TermEnd = DateTime.UtcNow.Date.AddMonths(3),
            };
            var youth = new User
            {
                Email = email,
                DisplayName = "Published Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CohortId = cohort.Id,
                PasswordHash = hasher.Hash(password),
            };
            db.Cohorts.Add(cohort);
            db.Users.Add(youth);
            db.GuardianConsents.Add(new GuardianConsent
            {
                YouthUserId = youth.Id,
                GuardianName = "Test Guardian",
                Relationship = "Parent",
                ParticipationConsent = true,
                Signature = "Test Guardian",
                ReferenceId = "CURRIC-PUBLISH",
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidUntil = DateTime.UtcNow.AddMonths(3),
            });
            await db.SaveChangesAsync();
        }
        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        var youthToken = (await signIn.Content.ReadFromJsonAsync<AuthTokenDto>())!.AccessToken;

        var hidden = await (await GetAsync("/api/me/curriculum", youthToken))
            .Content.ReadFromJsonAsync<CurriculumDto>();
        Assert.That(hidden!.Tracks, Is.Empty, "draft tracks must not appear to assigned youth");

        var publishTrack = await PostAsync(
            $"/api/tracks/{trackId}/publish",
            new { status = (int)PublishStatus.Published },
            adminToken);
        Assert.That(publishTrack.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var visible = await (await GetAsync("/api/me/curriculum", youthToken))
            .Content.ReadFromJsonAsync<CurriculumDto>();
        Assert.Multiple(() =>
        {
            Assert.That(visible!.Tracks.Single().TrackId, Is.EqualTo(trackId));
            Assert.That(visible.Modules.Single().ModuleId, Is.EqualTo(moduleId));
            Assert.That(visible.Modules.Single().Items.Single().ItemId, Is.EqualTo(lessonId));
        });
    }

    // Traces to: L2-CURRIC-001 (AC3 lesson ordering), L2-CURRIC-002 (AC1 lesson editor)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    [Property("Req", "L2-CURRIC-002")]
    public async Task Admin_edits_and_reorders_lessons_and_partial_reorders_are_rejected()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks",
            new { name = "Lesson Editing", key = $"lesson-edit-{Guid.NewGuid():N}" },
            adminToken)).Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync(
            "/api/modules",
            new { trackId, name = "Editor Module" },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var firstId = (await (await PostAsync(
            "/api/lessons",
            new { moduleId, title = "First draft" },
            adminToken)).Content.ReadFromJsonAsync<LessonResultDto>())!.Id;
        var secondId = (await (await PostAsync(
            "/api/lessons",
            new { moduleId, title = "Second draft" },
            adminToken)).Content.ReadFromJsonAsync<LessonResultDto>())!.Id;

        var update = await PutAsync($"/api/lessons/{secondId}", new
        {
            moduleId,
            title = "Serving with courage",
            scriptureReference = "Joshua 1:9",
            estimatedMinutes = 40,
            learningObjectives = "Practise courageous service.",
            teachingBody = "Updated teaching content.",
            reflectionPrompts = "Where do you need courage?",
            leaderGuideNotes = "Offer a private reflection option.",
        }, adminToken);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var reorder = await PostAsync(
            "/api/lessons/reorder",
            new { moduleId, orderedLessonIds = new[] { secondId, firstId } },
            adminToken);
        Assert.That(reorder.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var workspace = await (await GetAsync($"/api/tracks/{trackId}", adminToken))
            .Content.ReadFromJsonAsync<TrackWorkspaceDto>();
        var lessons = workspace!.Modules.Single().Lessons;
        Assert.Multiple(() =>
        {
            Assert.That(lessons.Select(item => item.Id), Is.EqualTo(new[] { secondId, firstId }));
            Assert.That(lessons[0].Title, Is.EqualTo("Serving with courage"));
            Assert.That(lessons[0].TeachingBody, Is.EqualTo("Updated teaching content."));
            Assert.That(lessons[0].LeaderGuideNotes, Is.EqualTo("Offer a private reflection option."));
        });

        var partial = await PostAsync(
            "/api/lessons/reorder",
            new { moduleId, orderedLessonIds = new[] { firstId } },
            adminToken);
        Assert.That(partial.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    // Traces to: L2-CURRIC-001 (AC2 prerequisites and AC3 module ordering)
    [Test]
    [Property("Req", "L2-CURRIC-001")]
    public async Task Admin_edits_and_reorders_modules_while_prerequisites_remain_in_the_same_track()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks",
            new { name = "Module Editing", key = $"module-edit-{Guid.NewGuid():N}" },
            adminToken)).Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var firstId = (await (await PostAsync(
            "/api/modules",
            new { trackId, name = "First Module" },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var secondId = (await (await PostAsync(
            "/api/modules",
            new { trackId, name = "Second Module" },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var thirdId = (await (await PostAsync(
            "/api/modules",
            new { trackId, name = "Third Module" },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var otherTrackId = (await (await PostAsync(
            "/api/tracks",
            new { name = "Other Track", key = $"other-module-{Guid.NewGuid():N}" },
            adminToken)).Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var foreignModuleId = (await (await PostAsync(
            "/api/modules",
            new { trackId = otherTrackId, name = "Foreign Module" },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var invalidCreate = await PostAsync(
            "/api/modules",
            new
            {
                trackId,
                name = "Invalid new module",
                prerequisiteModuleId = foreignModuleId,
            },
            adminToken);
        Assert.That(invalidCreate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var update = await PutAsync($"/api/modules/{secondId}", new
        {
            trackId,
            name = "Practising leadership",
            description = "Apply the foundation.",
            prerequisiteModuleId = firstId,
            scriptureAnchor = "James 1:22",
        }, adminToken);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var foreignPrerequisite = await PutAsync($"/api/modules/{secondId}", new
        {
            trackId,
            name = "Invalid prerequisite",
            prerequisiteModuleId = foreignModuleId,
        }, adminToken);
        Assert.That(foreignPrerequisite.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var invalidPrerequisiteOrder = await PostAsync(
            "/api/modules/reorder",
            new { trackId, orderedModuleIds = new[] { secondId, firstId, thirdId } },
            adminToken);
        Assert.That(invalidPrerequisiteOrder.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var reorder = await PostAsync(
            "/api/modules/reorder",
            new { trackId, orderedModuleIds = new[] { thirdId, firstId, secondId } },
            adminToken);
        Assert.That(reorder.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var workspace = await (await GetAsync($"/api/tracks/{trackId}", adminToken))
            .Content.ReadFromJsonAsync<TrackWorkspaceDto>();
        Assert.Multiple(() =>
        {
            Assert.That(workspace!.Modules.Select(item => item.Id),
                Is.EqualTo(new[] { thirdId, firstId, secondId }));
            Assert.That(workspace.Modules[2].Name, Is.EqualTo("Practising leadership"));
            Assert.That(workspace.Modules[2].PrerequisiteModuleId, Is.EqualTo(firstId));
        });

        var partial = await PostAsync(
            "/api/modules/reorder",
            new { trackId, orderedModuleIds = new[] { firstId, secondId } },
            adminToken);
        Assert.That(partial.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    // Traces to: L2-CURRIC-002 (AC2 validated media list/serve/remove), L2-NFR-001
    [Test]
    [Property("Req", "L2-CURRIC-002")]
    [Property("Req", "L2-NFR-001")]
    public async Task Lesson_media_is_signature_checked_listed_served_safely_and_removable()
    {
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync(
            "/api/tracks",
            new { name = "Media Track", key = $"media-{Guid.NewGuid():N}" },
            adminToken)).Content.ReadFromJsonAsync<TrackResultDto>())!.Id;
        var moduleId = (await (await PostAsync(
            "/api/modules",
            new { trackId, name = "Media Module" },
            adminToken)).Content.ReadFromJsonAsync<ModuleResultDto>())!.Id;
        var lessonId = (await (await PostAsync(
            "/api/lessons",
            new { moduleId, title = "Media Lesson" },
            adminToken)).Content.ReadFromJsonAsync<LessonResultDto>())!.Id;

        var disguised = await UploadAsync(
            $"/api/lessons/{lessonId}/media",
            "disguised.pdf",
            "application/pdf",
            "this is not a pdf"u8.ToArray(),
            adminToken);
        Assert.That(disguised.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var pdf = "%PDF-1.7\n1 0 obj\n<<>>\nendobj\n%%EOF"u8.ToArray();
        var uploaded = await UploadAsync(
            $"/api/lessons/{lessonId}/media",
            "leader-handout.pdf",
            "application/pdf",
            pdf,
            adminToken);
        Assert.That(uploaded.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await uploaded.Content.ReadFromJsonAsync<LessonMediaDto>();
        Assert.Multiple(() =>
        {
            Assert.That(result!.MediaType, Is.EqualTo("document"));
            Assert.That(result.FileName, Is.EqualTo("leader-handout.pdf"));
            Assert.That(result.SizeBytes, Is.EqualTo(pdf.Length));
            Assert.That(result.Url, Is.EqualTo($"/api/lesson-media/{result.Id}"));
        });

        var workspace = await (await GetAsync($"/api/tracks/{trackId}", adminToken))
            .Content.ReadFromJsonAsync<TrackWorkspaceDto>();
        Assert.That(workspace!.Modules.Single().Lessons.Single().Media.Single().Id, Is.EqualTo(result!.Id));

        var download = await GetAsync(result.Url, adminToken);
        Assert.Multiple(() =>
        {
            Assert.That(download.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(download.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/pdf"));
            Assert.That(download.Content.Headers.ContentDisposition?.DispositionType, Is.EqualTo("attachment"));
            Assert.That(download.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
            Assert.That(download.Content.ReadAsByteArrayAsync().Result, Is.EqualTo(pdf));
        });

        var removed = await DeleteAsync($"/api/lessons/{lessonId}/media/{result.Id}", adminToken);
        Assert.That(removed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await GetAsync(result.Url, adminToken)).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private sealed record TrackResultDto(Guid Id);
    private sealed record ModuleResultDto(Guid Id);
    private sealed record LessonResultDto(Guid Id);
    private sealed record LessonDetailDto(
        Guid Id,
        Guid ModuleId,
        string Title,
        string? ScriptureReference,
        int? EstimatedMinutes,
        string? LearningObjectives,
        string? TeachingBody,
        string? ReflectionPrompts,
        string? LeaderGuideNotes,
        string Status);
    private sealed record TrackWorkspaceDto(
        Guid Id,
        string Name,
        string Key,
        string? Description,
        string Status,
        bool Enabled,
        IReadOnlyList<string> AssignedCohortNames,
        IReadOnlyList<ModuleWorkspaceDto> Modules);
    private sealed record ModuleWorkspaceDto(
        Guid Id,
        string Name,
        string? Description,
        int Order,
        Guid? PrerequisiteModuleId,
        string? ScriptureAnchor,
        string Status,
        IReadOnlyList<LessonWorkspaceDto> Lessons,
        IReadOnlyList<QuizWorkspaceDto> Quizzes,
        IReadOnlyList<AssignmentWorkspaceDto> Assignments);
    private sealed record LessonWorkspaceDto(
        Guid Id,
        string Title,
        int Order,
        string Status,
        string? LeaderGuideNotes,
        string? TeachingBody,
        IReadOnlyList<LessonMediaDto> Media,
        IReadOnlyList<QuizWorkspaceDto> Quizzes,
        IReadOnlyList<AssignmentWorkspaceDto> Assignments);
    private sealed record QuizWorkspaceDto(
        Guid Id,
        int PassMark,
        int AttemptsAllowed,
        bool RequiredToCompleteModule,
        int QuestionCount);
    private sealed record AssignmentWorkspaceDto(
        Guid Id,
        string Title,
        int TotalPoints,
        string Status);
    private sealed record LessonMediaDto(
        Guid Id,
        string MediaType,
        string FileName,
        string ContentType,
        long SizeBytes,
        string Url);
    private sealed record AuthTokenDto(string AccessToken);
    private sealed record CurriculumDto(
        IReadOnlyList<TrackOptionDto> Tracks,
        IReadOnlyList<ModuleStatusDto> Modules);
    private sealed record TrackOptionDto(Guid TrackId);
    private sealed record ModuleStatusDto(Guid ModuleId, IReadOnlyList<CurriculumItemDto> Items);
    private sealed record CurriculumItemDto(Guid ItemId);
}

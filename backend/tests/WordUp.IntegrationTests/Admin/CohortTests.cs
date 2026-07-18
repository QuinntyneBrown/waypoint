using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Admin;

// Acceptance Test
// Traces to: L2-ADMIN-003 (cohort management), L2-ADMIN-004 (progress board)
[TestFixture]
[Property("Req", "L2-ADMIN-003")]
public sealed class CohortTests : IntegrationTestBase
{
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

    private Guid SeedTrack(PublishStatus status = PublishStatus.Published, string name = "Bible")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var track = new Track { Name = name, Key = $"track-{Guid.NewGuid():N}", Status = status, Enabled = true };
        db.Add(track);
        db.SaveChanges();
        return track.Id;
    }

    private Guid SeedUser(string role)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new User
        {
            Email = $"{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com",
            DisplayName = $"{role} Person",
            Role = role,
            Status = UserStatus.Active,
        };
        db.Add(user);
        db.SaveChanges();
        return user.Id;
    }

    [Test]
    [Property("Req", "L2-ADMIN-003")]
    public async Task Admin_creates_a_cohort_and_non_admin_is_forbidden()
    {
        // Traces to: L2-ADMIN-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts",
            new { name = "Fall 2026", trackId, leadMentorId = mentorId, location = "Lagos" }, adminToken);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await create.Content.ReadFromJsonAsync<CohortResultDto>();
        Assert.That(result!.Id, Is.Not.EqualTo(Guid.Empty));

        var detailResponse = await SendAsync(HttpMethod.Get, $"/api/cohorts/{result.Id}", null, adminToken);
        var detail = await detailResponse.Content.ReadFromJsonAsync<CohortDetailDto>();
        Assert.Multiple(() =>
        {
            Assert.That(detail!.Name, Is.EqualTo("Fall 2026"));
            Assert.That(detail.TrackName, Is.EqualTo("Bible"));
            Assert.That(detail.LeadMentorId, Is.EqualTo(mentorId));
            Assert.That(detail.Location, Is.EqualTo("Lagos"));
        });

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.That(await db.AuditEntries.AnyAsync(entry =>
                entry.Action == "CohortCreated" && entry.TargetId == result.Id), Is.True);
        }

        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await SendAsync(HttpMethod.Post, "/api/cohorts",
            new { name = "Nope", trackId, leadMentorId = mentorId }, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-ADMIN-003")]
    public async Task Create_requires_real_enabled_track_active_mentor_and_ordered_dates()
    {
        // Traces to: L2-ADMIN-003 AC1
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);

        var missingTrack = await SendAsync(HttpMethod.Post, "/api/cohorts", new
        {
            name = "Missing track",
            trackId = Guid.NewGuid(),
            leadMentorId = mentorId,
        }, adminToken);
        Assert.That(missingTrack.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var wrongMentor = await SendAsync(HttpMethod.Post, "/api/cohorts", new
        {
            name = "Wrong mentor",
            trackId,
            leadMentorId = SeedUser(Roles.Volunteer),
        }, adminToken);
        Assert.That(wrongMentor.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var reversedDates = await SendAsync(HttpMethod.Post, "/api/cohorts", new
        {
            name = "Reversed dates",
            trackId,
            leadMentorId = mentorId,
            startDate = new DateTime(2027, 1, 1),
            endDate = new DateTime(2026, 1, 1),
        }, adminToken);
        Assert.That(reversedDates.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Property("Req", "L2-ADMIN-003")]
    public async Task Adding_a_youth_to_a_cohort_is_reflected_in_the_member_count()
    {
        // Traces to: L2-ADMIN-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);
        var youthId = SeedUser(Roles.Youth);

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts",
            new { name = "Spring 2026", trackId, leadMentorId = mentorId }, adminToken);
        var cohortId = (await create.Content.ReadFromJsonAsync<CohortResultDto>())!.Id;

        var add = await SendAsync(HttpMethod.Post, $"/api/cohorts/{cohortId}/youth", new { youthId }, adminToken);
        Assert.That(add.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var list = await SendAsync(HttpMethod.Get, "/api/cohorts", null, adminToken);
        var cohorts = await list.Content.ReadFromJsonAsync<List<CohortSummaryDto>>();
        var summary = cohorts!.Single(c => c.Id == cohortId);
        Assert.That(summary.MemberCount, Is.EqualTo(1));

        var detailResponse = await SendAsync(HttpMethod.Get, $"/api/cohorts/{cohortId}", null, adminToken);
        var detail = await detailResponse.Content.ReadFromJsonAsync<CohortDetailDto>();
        Assert.That(detail!.Members.Select(member => member.Id), Does.Contain(youthId));
        Assert.That(detail.AvailableYouth.Select(youth => youth.Id), Does.Not.Contain(youthId));
    }

    [Test]
    [Property("Req", "L2-ADMIN-003")]
    public async Task Cohort_options_detail_and_audited_edit_support_the_management_screen()
    {
        // Traces to: L2-ADMIN-003 AC2/AC3
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);
        var replacementMentorId = SeedUser(Roles.Mentor);
        var youthId = SeedUser(Roles.Youth);

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts", new
        {
            name = "Detail cohort",
            trackId,
            leadMentorId = mentorId,
            startDate = new DateTime(2026, 9, 1),
            endDate = new DateTime(2027, 1, 15),
            location = "Old room",
        }, adminToken);
        var cohortId = (await create.Content.ReadFromJsonAsync<CohortResultDto>())!.Id;

        var optionsResponse = await SendAsync(HttpMethod.Get, "/api/cohorts/options", null, adminToken);
        var options = await optionsResponse.Content.ReadFromJsonAsync<CohortOptionsDto>();
        Assert.That(options!.Tracks.Select(track => track.Id), Does.Contain(trackId));
        Assert.That(options.Mentors.Select(mentor => mentor.Id), Does.Contain(replacementMentorId));

        var beforeResponse = await SendAsync(HttpMethod.Get, $"/api/cohorts/{cohortId}", null, adminToken);
        var before = await beforeResponse.Content.ReadFromJsonAsync<CohortDetailDto>();
        Assert.That(before!.AvailableYouth.Select(youth => youth.Id), Does.Contain(youthId));

        var edit = await SendAsync(HttpMethod.Post, $"/api/cohorts/{cohortId}/edit", new
        {
            name = "Detail cohort updated",
            leadMentorId = replacementMentorId,
            location = "Community hall",
        }, adminToken);
        Assert.That(edit.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterResponse = await SendAsync(HttpMethod.Get, $"/api/cohorts/{cohortId}", null, adminToken);
        var after = await afterResponse.Content.ReadFromJsonAsync<CohortDetailDto>();
        Assert.Multiple(() =>
        {
            Assert.That(after!.Name, Is.EqualTo("Detail cohort updated"));
            Assert.That(after.Location, Is.EqualTo("Community hall"));
            Assert.That(after.LeadMentorId, Is.EqualTo(replacementMentorId));
        });

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(await db.AuditEntries.AnyAsync(entry =>
            entry.Action == "CohortUpdated" && entry.TargetId == cohortId), Is.True);
    }

    [Test]
    [Property("Req", "L2-ADMIN-003")]
    public async Task Adding_members_rejects_non_youth_and_youth_already_in_another_cohort()
    {
        // Traces to: L2-ADMIN-003 AC2
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);
        var volunteerId = SeedUser(Roles.Volunteer);
        var placedYouthId = SeedUser(Roles.Youth);
        Guid otherCohortId;

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts", new
        {
            name = "Membership target",
            trackId,
            leadMentorId = mentorId,
        }, adminToken);
        var cohortId = (await create.Content.ReadFromJsonAsync<CohortResultDto>())!.Id;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var other = new Cohort { Name = "Other cohort" };
            db.Cohorts.Add(other);
            (await db.Users.SingleAsync(user => user.Id == placedYouthId)).CohortId = other.Id;
            await db.SaveChangesAsync();
            otherCohortId = other.Id;
        }

        var wrongRole = await SendAsync(
            HttpMethod.Post,
            $"/api/cohorts/{cohortId}/youth",
            new { youthId = volunteerId },
            adminToken);
        Assert.That(wrongRole.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var alreadyPlaced = await SendAsync(
            HttpMethod.Post,
            $"/api/cohorts/{cohortId}/youth",
            new { youthId = placedYouthId },
            adminToken);
        Assert.That(alreadyPlaced.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        using var verifyScope = Factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That((await verifyDb.Users.SingleAsync(user => user.Id == placedYouthId)).CohortId,
            Is.EqualTo(otherCohortId));
    }

    [Test]
    [Property("Req", "L2-ADMIN-004")]
    public async Task Progress_board_returns_youth_rows_and_published_module_columns()
    {
        // Traces to: L2-ADMIN-004
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = SeedTrack();
        var mentorId = SeedUser(Roles.Mentor);

        Guid moduleId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var module = new Module
            {
                TrackId = trackId,
                Name = "Genesis",
                Order = 1,
                Status = PublishStatus.Published,
            };
            db.Add(module);
            db.SaveChanges();
            moduleId = module.Id;
        }

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts",
            new { name = "Progress Cohort", trackId, leadMentorId = mentorId }, adminToken);
        var cohortId = (await create.Content.ReadFromJsonAsync<CohortResultDto>())!.Id;

        var youthId = SeedUser(Roles.Youth);
        var add = await SendAsync(HttpMethod.Post, $"/api/cohorts/{cohortId}/youth", new { youthId }, adminToken);
        Assert.That(add.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var progress = await SendAsync(HttpMethod.Get, $"/api/cohorts/{cohortId}/progress", null, adminToken);
        Assert.That(progress.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var board = await progress.Content.ReadFromJsonAsync<ProgressBoardDto>();
        Assert.That(board!.Modules.Select(m => m.ModuleId), Does.Contain(moduleId));
        Assert.That(board.Rows.Select(r => r.YouthId), Does.Contain(youthId));
        Assert.That(board.CohortAveragePercent, Is.EqualTo(0));

        var row = board.Rows.Single(r => r.YouthId == youthId);
        Assert.That(row.Cells.Single(c => c.ModuleId == moduleId).State, Is.EqualTo("NotStarted"));
    }

    [Test]
    [Property("Req", "L2-ADMIN-004")]
    public async Task Progress_board_computes_selected_track_states_averages_and_filtered_exports()
    {
        // Traces to: L2-ADMIN-004 AC1/AC3, L2-REPORT-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var primaryTrackId = SeedTrack(name: "Primary track");
        var selectedTrackId = SeedTrack(name: "Leadership track");
        var mentorId = SeedUser(Roles.Mentor);
        var assignedYouthId = SeedUser(Roles.Youth);
        var unassignedYouthId = SeedUser(Roles.Youth);
        Guid primaryModuleId;
        Guid selectedModuleId;

        var create = await SendAsync(HttpMethod.Post, "/api/cohorts", new
        {
            name = "Multi-track progress",
            trackId = primaryTrackId,
            leadMentorId = mentorId,
        }, adminToken);
        var cohortId = (await create.Content.ReadFromJsonAsync<CohortResultDto>())!.Id;
        await SendAsync(HttpMethod.Post, $"/api/cohorts/{cohortId}/youth", new { youthId = assignedYouthId }, adminToken);
        await SendAsync(HttpMethod.Post, $"/api/cohorts/{cohortId}/youth", new { youthId = unassignedYouthId }, adminToken);

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var primaryModule = new Module
            {
                TrackId = primaryTrackId,
                Name = "Primary module",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var selectedModule = new Module
            {
                TrackId = selectedTrackId,
                Name = "Leadership module",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var lessonOne = new Lesson
            {
                ModuleId = selectedModule.Id,
                Title = "Lead with care",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var lessonTwo = new Lesson
            {
                ModuleId = selectedModule.Id,
                Title = "Serve the community",
                Order = 2,
                Status = PublishStatus.Published,
            };
            var quiz = new Quiz { ModuleId = selectedModule.Id };
            db.Modules.AddRange(primaryModule, selectedModule);
            db.Lessons.AddRange(lessonOne, lessonTwo);
            db.Quizzes.Add(quiz);
            db.YouthTrackEnrollments.Add(new YouthTrackEnrollment
            {
                YouthUserId = assignedYouthId,
                TrackId = selectedTrackId,
            });
            db.LessonProgress.AddRange(
                new LessonProgress
                {
                    YouthUserId = assignedYouthId,
                    LessonId = lessonOne.Id,
                    Status = ProgressStatus.Completed,
                },
                new LessonProgress
                {
                    YouthUserId = assignedYouthId,
                    LessonId = lessonTwo.Id,
                    Status = ProgressStatus.Completed,
                });
            db.QuizAttempts.Add(new QuizAttempt
            {
                YouthUserId = assignedYouthId,
                QuizId = quiz.Id,
                AttemptNumber = 1,
                AutoScore = 80,
                ApprovedScore = 88,
                ApprovedAt = DateTime.UtcNow,
                IsSubmitted = true,
            });
            await db.SaveChangesAsync();
            primaryModuleId = primaryModule.Id;
            selectedModuleId = selectedModule.Id;
        }

        var progress = await SendAsync(
            HttpMethod.Get,
            $"/api/cohorts/{cohortId}/progress?trackId={selectedTrackId}",
            null,
            adminToken);
        Assert.That(progress.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var board = await progress.Content.ReadFromJsonAsync<ProgressBoardDto>();
        var assigned = board!.Rows.Single(row => row.YouthId == assignedYouthId);
        var unassigned = board.Rows.Single(row => row.YouthId == unassignedYouthId);
        Assert.Multiple(() =>
        {
            Assert.That(board.TrackId, Is.EqualTo(selectedTrackId));
            Assert.That(board.TrackName, Is.EqualTo("Leadership track"));
            Assert.That(board.Modules.Select(module => module.ModuleId),
                Is.EqualTo(new[] { selectedModuleId }));
            Assert.That(board.Modules.Select(module => module.ModuleId), Does.Not.Contain(primaryModuleId));
            Assert.That(assigned.Cells.Single().State, Is.EqualTo("Done"));
            Assert.That(assigned.QuizAverage, Is.EqualTo(88));
            Assert.That(assigned.OverallPercent, Is.EqualTo(1));
            Assert.That(unassigned.Cells.Single().State, Is.EqualTo("NotAssigned"));
            Assert.That(board.Modules.Single().AveragePercent, Is.EqualTo(1));
            Assert.That(board.CohortAveragePercent, Is.EqualTo(1));
        });

        var csv = await SendAsync(
            HttpMethod.Get,
            $"/api/cohorts/{cohortId}/progress/export?format=csv&trackId={selectedTrackId}",
            null,
            adminToken);
        Assert.That(csv.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var csvContent = await csv.Content.ReadAsStringAsync();
        Assert.That(csvContent, Does.Contain("Leadership module"));
        Assert.That(csvContent, Does.Contain("Done"));
        Assert.That(csvContent, Does.Not.Contain("Primary module"));

        var pdf = await SendAsync(
            HttpMethod.Get,
            $"/api/cohorts/{cohortId}/progress/export?format=pdf&trackId={selectedTrackId}",
            null,
            adminToken);
        Assert.That(pdf.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var pdfBytes = await pdf.Content.ReadAsByteArrayAsync();
        Assert.That(System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 8), Does.StartWith("%PDF-1.4"));
    }

    private sealed record CohortResultDto(Guid Id);
    private sealed record CohortSummaryDto(
        Guid Id,
        string Name,
        Guid? TrackId,
        string? TrackName,
        Guid? MentorId,
        string? MentorName,
        DateTime StartDate,
        DateTime EndDate,
        string? Location,
        int MemberCount);
    private sealed record CohortDetailDto(
        Guid Id,
        string Name,
        Guid? TrackId,
        string? TrackName,
        Guid? LeadMentorId,
        string? LeadMentorName,
        DateTime StartDate,
        DateTime EndDate,
        string? Location,
        int MemberCount,
        IReadOnlyList<CohortYouthDto> Members,
        IReadOnlyList<CohortYouthDto> AvailableYouth);
    private sealed record CohortYouthDto(Guid Id, string Name, string Email);
    private sealed record CohortOptionsDto(
        IReadOnlyList<CohortOptionDto> Tracks,
        IReadOnlyList<CohortOptionDto> Mentors);
    private sealed record CohortOptionDto(Guid Id, string Name);
    private sealed record ProgressBoardDto(
        Guid CohortId,
        string CohortName,
        Guid TrackId,
        string TrackName,
        List<ModuleColumnDto> Modules,
        List<YouthRowDto> Rows,
        double CohortAveragePercent);
    private sealed record ModuleColumnDto(Guid ModuleId, string Name, double AveragePercent);
    private sealed record YouthRowDto(
        Guid YouthId,
        string YouthName,
        List<CellStateDto> Cells,
        double QuizAverage,
        double OverallPercent);
    private sealed record CellStateDto(Guid ModuleId, string State);
}

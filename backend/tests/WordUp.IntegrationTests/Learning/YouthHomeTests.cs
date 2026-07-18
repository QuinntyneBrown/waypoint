using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-001..010, L2-ENGAGE-005, L2-MENTOR-008, L2-PLATFORM-011
[TestFixture]
[Property("Req", "L2-LEARN-001")]
[Property("Req", "L2-ENGAGE-005")]
public sealed class YouthHomeTests : IntegrationTestBase
{
    [Test]
    public async Task Placed_youth_home_combines_current_learning_progress_next_session_and_assigned_work()
    {
        var now = DateTime.UtcNow;
        var password = "Passw0rd!";
        var email = $"home.{Guid.NewGuid():N}@example.com";
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var mentor = new User
            {
                Email = $"mentor.{Guid.NewGuid():N}@example.com",
                DisplayName = "Jordan Mentor",
                Role = Roles.Mentor,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(password),
            };
            var track = new Track
            {
                Name = "Leadership",
                Key = $"leadership-{Guid.NewGuid():N}",
                Enabled = true,
                Status = PublishStatus.Published,
            };
            var module = new Module
            {
                TrackId = track.Id,
                Name = "Serving well",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var first = new Lesson
            {
                ModuleId = module.Id,
                Title = "Start with listening",
                Order = 1,
                Status = PublishStatus.Published,
            };
            var next = new Lesson
            {
                ModuleId = module.Id,
                Title = "Lead through service",
                Order = 2,
                EstimatedMinutes = 20,
                Status = PublishStatus.Published,
            };
            var cohort = new Cohort
            {
                Name = "Northside Leaders",
                TrackId = track.Id,
                MentorId = mentor.Id,
                TermStart = now.AddDays(-30),
                TermEnd = now.AddDays(90),
            };
            var youth = new User
            {
                Email = email,
                DisplayName = "Home Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(password),
                CohortId = cohort.Id,
                MentorId = mentor.Id,
            };
            var assignment = new Assignment
            {
                ModuleId = module.Id,
                Title = "Neighbourhood plan",
                TaskDescription = "Plan a service activity.",
                SubmissionTypes = "text",
            };
            var session = new Session
            {
                CohortId = cohort.Id,
                MentorId = mentor.Id,
                Title = "Leadership circle",
                StartsAt = now.AddDays(2),
                DurationMinutes = 60,
                Location = "Community Hub",
                CreatedByUserId = mentor.Id,
            };
            db.Users.AddRange(mentor, youth);
            db.Tracks.Add(track);
            db.Modules.Add(module);
            db.Lessons.AddRange(first, next);
            db.Cohorts.Add(cohort);
            db.Assignments.Add(assignment);
            db.Sessions.Add(session);
            db.LessonProgress.Add(new LessonProgress
            {
                YouthUserId = youth.Id,
                LessonId = first.Id,
                Status = ProgressStatus.Completed,
                CompletedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1),
            });
            db.AssignedWork.Add(new AssignedWork
            {
                ItemType = "Project",
                ItemId = assignment.Id,
                YouthUserId = youth.Id,
                DueDate = now.AddDays(5),
                Message = "Bring your ideas.",
                AssignedByUserId = mentor.Id,
            });
            await db.SaveChangesAsync();
        }

        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        var auth = await signIn.Content.ReadFromJsonAsync<AuthResult>();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/me/home");
        Authorize(request, auth!.AccessToken);
        var response = await Client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var home = await response.Content.ReadFromJsonAsync<HomeDto>();
        Assert.Multiple(() =>
        {
            Assert.That(home!.Placed, Is.True);
            Assert.That(home.CohortName, Is.EqualTo("Northside Leaders"));
            Assert.That(home.MentorName, Is.EqualTo("Jordan Mentor"));
            Assert.That(home.CompletedItems, Is.EqualTo(1));
            Assert.That(home.TotalItems, Is.EqualTo(2));
            Assert.That(home.OverallPercent, Is.EqualTo(50));
            Assert.That(home.ContinueLearning!.Title, Is.EqualTo("Lead through service"));
            Assert.That(home.ContinueLearning.TargetPath, Does.StartWith("/youth/lessons/"));
            Assert.That(home.NextSession!.Title, Is.EqualTo("Leadership circle"));
            Assert.That(home.AssignedWork.Single().Title, Is.EqualTo("Neighbourhood plan"));
            Assert.That(home.AssignedWork.Single().TargetPath, Does.StartWith("/youth/assignments/"));
        });
    }

    private sealed record HomeDto(
        bool Placed,
        string? CohortName,
        string? MentorName,
        int CompletedItems,
        int TotalItems,
        double OverallPercent,
        HomeLearningDto? ContinueLearning,
        HomeSessionDto? NextSession,
        List<HomeWorkDto> AssignedWork);
    private sealed record HomeLearningDto(string Title, string TargetPath);
    private sealed record HomeSessionDto(string Title);
    private sealed record HomeWorkDto(string Title, string TargetPath);
}

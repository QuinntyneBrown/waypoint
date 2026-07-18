using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Engagement;

// Acceptance Test
// Traces to: L2-ENGAGE-005 (session RSVP: youth responds, optional note delivered to mentor, one RSVP per youth)
[TestFixture]
[Property("Req", "L2-ENGAGE-005")]
public sealed class RsvpTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null, string? token = null)
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

    private async Task<(Guid Id, string Email)> SeedUserAsync(string role, Guid? mentorId = null)
    {
        var email = $"seed.{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = email,
            DisplayName = $"Seed {role}",
            Role = role,
            Status = UserStatus.Active,
            MentorId = mentorId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task<Guid> SeedSessionAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = new Session
        {
            CohortId = Guid.NewGuid(),
            Title = "Weekly Session",
            StartsAt = DateTime.UtcNow.AddDays(1),
            CreatedByUserId = Guid.NewGuid(),
        };
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session.Id;
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-ENGAGE-005")]
    public async Task Youth_rsvps_with_a_note_and_reminder_the_mentor_sees_it_and_re_rsvp_updates_in_place()
    {
        // Traces to: L2-ENGAGE-005
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var sessionId = await SeedSessionAsync();
        var youthToken = await SignInAsync(youth.Email);
        var mentorToken = await SignInAsync(mentor.Email);

        var rsvp = await SendAsync(HttpMethod.Post, $"/api/sessions/{sessionId}/rsvp", body: new
        {
            response = "going",
            note = "Bringing a friend from school.",
            reminderSet = true,
        }, token: youthToken);
        Assert.That(rsvp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var list = await SendAsync(HttpMethod.Get, $"/api/sessions/{sessionId}/rsvps", token: mentorToken);
        Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var views = await list.Content.ReadFromJsonAsync<List<RsvpDto>>();
        var mine = views!.Single(v => v.YouthUserId == youth.Id);
        Assert.That(mine.Response, Is.EqualTo("Going"));
        Assert.That(mine.Note, Is.EqualTo("Bringing a friend from school."));
        Assert.That(mine.ReminderSet, Is.True);

        // Re-submitting updates the existing RSVP rather than creating a duplicate.
        var change = await SendAsync(HttpMethod.Post, $"/api/sessions/{sessionId}/rsvp", body: new
        {
            response = "can't make it",
            note = "Something came up.",
            reminderSet = false,
        }, token: youthToken);
        Assert.That(change.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var after = await SendAsync(HttpMethod.Get, $"/api/sessions/{sessionId}/rsvps", token: mentorToken);
        var afterViews = await after.Content.ReadFromJsonAsync<List<RsvpDto>>();
        var updated = afterViews!.Where(v => v.YouthUserId == youth.Id).ToList();
        Assert.That(updated.Count, Is.EqualTo(1), "re-RSVP must update in place, not add a row");
        Assert.That(updated[0].Response, Is.EqualTo("CantAttend"));
        Assert.That(updated[0].Note, Is.EqualTo("Something came up."));
        Assert.That(updated[0].ReminderSet, Is.False);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rows = db.SessionRsvps.Count(r => r.SessionId == sessionId && r.YouthUserId == youth.Id);
        Assert.That(rows, Is.EqualTo(1));
    }

    private sealed record RsvpDto(Guid YouthUserId, string Response, string? Note, bool ReminderSet);
}

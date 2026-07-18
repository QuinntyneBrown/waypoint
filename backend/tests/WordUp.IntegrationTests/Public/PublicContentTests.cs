// Acceptance Test
// Traces to: L2-PUB-001 (public pages reachable without auth; no youth/guardian/mentor PII leak),
//            L2-PUB-002 (tracks with modules + outcome badges; cohort list with authoritative status)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Public;

[TestFixture]
[Property("Req", "L2-PUB-001")]
public sealed class PublicContentTests : IntegrationTestBase
{
    private sealed record SeededContent(
        Guid PublishedTrackId,
        string PublishedTrackName,
        string DraftTrackName,
        Guid PublishedModuleId,
        string PublishedModuleName,
        string DraftModuleName,
        string OutcomeBadgeName,
        string MentorDisplayName,
        string UpcomingNeighbourhood,
        string YouthEmail,
        string YouthDisplayName);

    private async Task<SeededContent> SeedPublishedContentAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var suffix = Guid.NewGuid().ToString("N");

        var publishedTrack = new Track
        {
            Name = $"Bible & Leadership {suffix}",
            Key = $"bible-leadership-{suffix}",
            Description = "Grow in faith and leadership.",
            Status = PublishStatus.Published,
            Enabled = true,
        };
        var draftTrack = new Track
        {
            Name = $"Secret Draft Track {suffix}",
            Key = $"draft-{suffix}",
            Status = PublishStatus.Draft,
            Enabled = true,
        };
        db.Tracks.AddRange(publishedTrack, draftTrack);

        var publishedModule = new Module
        {
            TrackId = publishedTrack.Id,
            Name = $"Foundations {suffix}",
            Description = "Where we begin.",
            Order = 1,
            Status = PublishStatus.Published,
        };
        var draftModule = new Module
        {
            TrackId = publishedTrack.Id,
            Name = $"Hidden Draft Module {suffix}",
            Order = 2,
            Status = PublishStatus.Draft,
        };
        db.Modules.AddRange(publishedModule, draftModule);

        var outcomeBadge = new Badge
        {
            Name = $"Foundations Badge {suffix}",
            Icon = "star",
            Description = "Awarded on module completion.",
            CriteriaKind = BadgeCriteriaKind.Auto,
            AutoRule = "module-completed",
            TargetRefId = publishedModule.Id,
        };
        db.Badges.Add(outcomeBadge);

        var mentor = new User
        {
            Email = $"mentor.{suffix}@example.com",
            DisplayName = $"Pat Mentor {suffix}",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        db.Users.Add(mentor);

        var upcomingCohort = new Cohort
        {
            Name = $"Upcoming Cohort {suffix}",
            Location = $"Regent Park {suffix}",
            TermStart = new DateTime(2999, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TermEnd = new DateTime(2999, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            MentorId = mentor.Id,
        };
        var completedCohort = new Cohort
        {
            Name = $"Completed Cohort {suffix}",
            Location = $"Jane & Finch {suffix}",
            TermStart = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TermEnd = new DateTime(2000, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            MentorId = mentor.Id,
        };
        db.Cohorts.AddRange(upcomingCohort, completedCohort);

        // A youth placed in the cohort — their PII must never appear in public content (L2-PUB-001 AC2).
        var youth = new User
        {
            Email = $"secret.youth.{suffix}@example.com",
            DisplayName = $"Secret Youth {suffix}",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CohortId = upcomingCohort.Id,
            MentorId = mentor.Id,
        };
        db.Users.Add(youth);

        await db.SaveChangesAsync();

        return new SeededContent(
            publishedTrack.Id,
            publishedTrack.Name,
            draftTrack.Name,
            publishedModule.Id,
            publishedModule.Name,
            draftModule.Name,
            outcomeBadge.Name,
            mentor.DisplayName,
            upcomingCohort.Location!,
            youth.Email,
            youth.DisplayName);
    }

    // Traces to: L2-PUB-001 (AC1 reachable anonymously; AC2 no PII leak), L2-PUB-002 (AC1/AC2)
    [Test]
    [Property("Req", "L2-PUB-001")]
    [Property("Req", "L2-PUB-002")]
    public async Task Public_content_is_served_anonymously_with_published_tracks_and_cohort_statuses()
    {
        var seed = await SeedPublishedContentAsync();

        // No Authorization header at all — the endpoint must be reachable anonymously (L2-PUB-001 AC1).
        var response = await Client.GetAsync("/api/public/content");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var raw = await response.Content.ReadAsStringAsync();
        var content = await response.Content.ReadFromJsonAsync<ContentResponse>();
        Assert.That(content, Is.Not.Null);

        // L2-PUB-002 AC1: published track present with its published module + outcome badge.
        var track = content!.Tracks.SingleOrDefault(t => t.Id == seed.PublishedTrackId);
        Assert.That(track, Is.Not.Null, "Published track should be exposed.");
        Assert.That(track!.Modules.Any(m => m.Id == seed.PublishedModuleId), Is.True);
        var module = track.Modules.Single(m => m.Id == seed.PublishedModuleId);
        Assert.That(module.OutcomeBadge, Is.Not.Null);
        Assert.That(module.OutcomeBadge!.Name, Is.EqualTo(seed.OutcomeBadgeName));

        // Draft track and draft module must not be exposed publicly (L2-PUB-002).
        Assert.That(content.Tracks.Any(t => t.Name == seed.DraftTrackName), Is.False);
        Assert.That(track.Modules.Any(m => m.Name == seed.DraftModuleName), Is.False);

        // L2-PUB-002 AC2/AC4: cohort list exposes neighbourhood, lead mentor, and a single authoritative status.
        var upcoming = content.Cohorts.Single(c => c.Neighbourhood == seed.UpcomingNeighbourhood);
        Assert.That(upcoming.Status, Is.EqualTo("Upcoming"));
        Assert.That(upcoming.LeadMentor, Is.EqualTo(seed.MentorDisplayName));
        var completed = content.Cohorts.Single(c => c.Status == "Completed");
        Assert.That(completed, Is.Not.Null);

        // The 4-step journey and impact stats are present (L2-PUB-002 AC3).
        Assert.That(content.Journey, Has.Count.EqualTo(4));
        Assert.That(content.Impact.Tracks, Is.GreaterThanOrEqualTo(1));

        // L2-PUB-001 AC2: no youth PII (email or display name) leaks into any public payload.
        Assert.That(raw, Does.Not.Contain(seed.YouthEmail));
        Assert.That(raw, Does.Not.Contain(seed.YouthDisplayName));
    }

    private sealed record ContentResponse(
        List<TrackDto> Tracks,
        List<CohortDto> Cohorts,
        MentorshipDto Mentorship,
        List<JourneyDto> Journey,
        ImpactDto Impact);

    private sealed record TrackDto(Guid Id, string Name, string Key, string? Description, List<ModuleDto> Modules);

    private sealed record ModuleDto(Guid Id, string Name, string? Description, int Order, BadgeDto? OutcomeBadge);

    private sealed record BadgeDto(Guid Id, string Name, string Icon, string? Description);

    private sealed record CohortDto(
        Guid Id, string Name, string? Neighbourhood, DateTime TermStart, DateTime TermEnd, string? LeadMentor, string Status);

    private sealed record MentorshipDto(string Summary, List<string> Features);

    private sealed record JourneyDto(int Step, string Title, string Description);

    private sealed record ImpactDto(int Tracks, int Cohorts, int Modules, int Neighbourhoods);
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;
using WordUp.Application.ConsentAndSafeguarding;

namespace WordUp.Application.ProgramAdministration.People;

public class GetPersonQueryHandler : IRequestHandler<GetPersonQuery, PersonDetail>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public GetPersonQueryHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<PersonDetail> Handle(GetPersonQuery request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");

        var cohort = user.CohortId is { } cohortId
            ? await _db.Cohorts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == cohortId, cancellationToken)
            : null;
        var mentor = user.MentorId is { } mentorId
            ? await _db.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == mentorId, cancellationToken)
            : null;
        var guardianUser = user.GuardianUserId is { } guardianId
            ? await _db.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == guardianId, cancellationToken)
            : null;
        var guardianProfile = guardianUser is null
            ? null
            : await _db.GuardianProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.GuardianUserId == guardianUser.Id, cancellationToken);

        var consents = await _db.GuardianConsents
            .AsNoTracking()
            .Where(item => item.YouthUserId == user.Id)
            .OrderByDescending(item => item.SignedAt)
            .ToListAsync(cancellationToken);
        var currentConsent = consents.FirstOrDefault();
        var consentPolicy = await ConsentPolicyEvaluator.LoadAsync(
            _db,
            _clock.UtcNow,
            cancellationToken);
        var hasValidConsent = consents.Any(item =>
            consentPolicy.IsParticipationValid(item, _clock.UtcNow));
        var guardian = guardianUser is null
            ? null
            : new PersonGuardianContact(
                guardianUser.DisplayName,
                guardianUser.Email,
                guardianProfile?.Relationship ?? currentConsent?.Relationship,
                guardianProfile?.Phone,
                guardianProfile?.EmergencyContactName,
                guardianProfile?.EmergencyContactPhone);
        var consent = new PersonConsentSummary(
            hasValidConsent,
            currentConsent?.ParticipationConsent ?? false,
            currentConsent?.MediaConsent ?? false,
            currentConsent?.SignedAt,
            currentConsent?.ValidUntil,
            currentConsent?.WithdrawnAt,
            currentConsent?.GuardianName,
            currentConsent?.Relationship);

        var trackIds = await _db.YouthTrackEnrollments
            .AsNoTracking()
            .Where(item => item.YouthUserId == user.Id)
            .Select(item => item.TrackId)
            .ToListAsync(cancellationToken);
        if (cohort?.TrackId is { } cohortTrackId && !trackIds.Contains(cohortTrackId))
        {
            trackIds.Add(cohortTrackId);
        }
        var moduleIds = await _db.Modules
            .AsNoTracking()
            .Where(item => trackIds.Contains(item.TrackId) && item.Status == PublishStatus.Published)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var lessonIds = await _db.Lessons
            .AsNoTracking()
            .Where(item => moduleIds.Contains(item.ModuleId)
                && item.Status == PublishStatus.Published
                && item.ArchivedAt == null)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var completedLessons = await _db.LessonProgress
            .AsNoTracking()
            .CountAsync(item => item.YouthUserId == user.Id
                && lessonIds.Contains(item.LessonId)
                && item.Status == ProgressStatus.Completed,
                cancellationToken);
        var progress = new PersonProgressSummary(
            completedLessons,
            lessonIds.Count,
            lessonIds.Count == 0 ? 0 : (double)completedLessons / lessonIds.Count);

        var attendanceRows = await _db.AttendanceRecords
            .AsNoTracking()
            .Where(item => item.YouthUserId == user.Id)
            .OrderByDescending(item => item.RecordedAt)
            .Take(100)
            .ToListAsync(cancellationToken);
        var sessionIds = attendanceRows.Select(item => item.SessionId).Distinct().ToList();
        var sessions = await _db.Sessions
            .AsNoTracking()
            .Where(item => sessionIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var attendanceItems = attendanceRows
            .Where(item => sessions.ContainsKey(item.SessionId))
            .Select(item => new PersonAttendanceItem(
                item.SessionId,
                sessions[item.SessionId].Title,
                sessions[item.SessionId].StartsAt,
                item.Status.ToString(),
                item.Note))
            .OrderByDescending(item => item.StartsAt)
            .ToList();
        var attended = attendanceItems.Count(item =>
            item.Status == AttendanceStatus.Present.ToString()
            || item.Status == AttendanceStatus.Late.ToString());
        var attendance = new PersonAttendanceSummary(
            attended,
            attendanceItems.Count,
            attendanceItems.Count == 0 ? 0 : (double)attended / attendanceItems.Count,
            attendanceItems);

        var checkIns = await _db.CheckIns
            .AsNoTracking()
            .Where(item => item.YouthUserId == user.Id)
            .OrderByDescending(item => item.Date)
            .Take(100)
            .ToListAsync(cancellationToken);
        var noteMentorIds = checkIns.Select(item => item.MentorUserId).Distinct().ToList();
        var noteMentorNames = await _db.Users
            .AsNoTracking()
            .Where(item => noteMentorIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.DisplayName, cancellationToken);
        var notes = checkIns.Select(item => new PersonNoteSummary(
                item.Id,
                item.Date,
                item.Type,
                noteMentorNames.GetValueOrDefault(item.MentorUserId, "Mentor"),
                BuildSafeSummary(item.Engagement, item.GoalsDiscussed, item.ActionItems),
                item.NextFollowUp))
            .ToList();
        var photoUrl = user.PhotoUrl;
        if (photoUrl is not null
            && user.Role == Roles.Youth
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                user.Id,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            photoUrl = null;
        }

        return new PersonDetail(
            user.Id,
            user.DisplayName,
            user.Email,
            user.Role,
            user.Status.ToString(),
            user.Pronouns,
            photoUrl,
            user.OnboardingCompleted,
            user.CreatedAt,
            user.LastSignInAt,
            user.CohortId,
            cohort?.Name,
            user.MentorId,
            mentor?.DisplayName,
            user.GuardianUserId,
            guardian,
            hasValidConsent,
            consent,
            progress,
            attendance,
            notes);
    }

    private static string BuildSafeSummary(
        string? engagement,
        string? goalsDiscussed,
        string? actionItems)
    {
        var parts = new[] { engagement, goalsDiscussed, actionItems }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim());
        var summary = string.Join(" · ", parts);
        return string.IsNullOrWhiteSpace(summary) ? "Check-in recorded." : summary;
    }
}

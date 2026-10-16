using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.PlatformServices.Privacy;

/// <summary>
/// Builds the authenticated subject-access export after an administrator has completed at least one
/// access request for the caller. The export deliberately omits authentication secrets and replaces
/// third-party author identities with relationship labels.
/// </summary>
public sealed record PersonalDataExportQuery : IRequest<PersonalDataExportResult>;

public sealed record PersonalDataExportResult(string FileName, byte[] Content);

public sealed class PersonalDataExportQueryHandler
    : IRequestHandler<PersonalDataExportQuery, PersonalDataExportResult>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public PersonalDataExportQueryHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
    }

    public async Task<PersonalDataExportResult> Handle(
        PersonalDataExportQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var user = await _db.Users.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var accessGrant = await _db.DataSubjectRequests.AsNoTracking()
            .Where(item => item.SubjectUserId == userId
                && item.ActionType == "access"
                && item.Status == DataSubjectRequestStatus.Completed)
            .OrderByDescending(item => item.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (accessGrant is null)
        {
            throw new ForbiddenException(
                "A completed data-access request is required before an export can be downloaded.");
        }

        var generatedAt = _clock.UtcNow;

        // Application access requires an explicit authenticated applicant/guardian link. Submitted
        // email equality is not identity proof and must never expose another person's application.
        var applications = await _db.Applications.AsNoTracking()
            .Where(item => item.CreatedYouthUserId == userId
                || item.ApplicantUserId == userId
                || item.GuardianUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var guardianConsents = await _db.GuardianConsents.AsNoTracking()
            .Where(item => item.YouthUserId == userId || item.GuardianUserId == userId)
            .OrderBy(item => item.SignedAt)
            .ToListAsync(cancellationToken);
        var guardianConsentRequests = await _db.GuardianConsentRequests.AsNoTracking()
            .Where(item => item.YouthUserId == userId || item.GuardianUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var guardianProfile = await _db.GuardianProfiles.AsNoTracking()
            .SingleOrDefaultAsync(item => item.GuardianUserId == userId, cancellationToken);

        var attendance = await _db.AttendanceRecords.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.RecordedAt)
            .ToListAsync(cancellationToken);
        var attendanceReceipts = await _db.AttendanceWriteReceipts.AsNoTracking()
            .Where(item => item.OwnerUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var rsvps = await _db.SessionRsvps.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var announcementReads = await _db.AnnouncementReads.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.ReadAt)
            .ToListAsync(cancellationToken);

        var lessonProgress = await _db.LessonProgress.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.UpdatedAt)
            .ToListAsync(cancellationToken);
        var codingWorkspaces = await _db.CodingLessonWorkspaces.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.UpdatedAt)
            .ToListAsync(cancellationToken);
        var trackEnrollments = await _db.YouthTrackEnrollments.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.EnrolledAt)
            .ToListAsync(cancellationToken);
        var readingStates = await _db.LessonReadingStates.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.UpdatedAt)
            .ToListAsync(cancellationToken);
        var quizAttempts = await _db.QuizAttempts.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var submissions = await _db.Submissions.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var submissionFiles = await _db.StoredSubmissionFiles.AsNoTracking()
            .Where(item => item.OwnerUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var submissionIds = submissions.Select(item => item.Id).ToArray();
        var submissionTimeline = submissionIds.Length == 0
            ? []
            : await _db.SubmissionTimelineEvents.AsNoTracking()
                .Where(item => submissionIds.Contains(item.SubmissionId))
                .OrderBy(item => item.OccurredAt)
                .ToListAsync(cancellationToken);
        var reflections = await _db.Reflections.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var badgeAwards = await _db.BadgeAwards.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.AwardedAt)
            .ToListAsync(cancellationToken);

        var assessments = await _db.CompetencyAssessments.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.AssessedAt)
            .ToListAsync(cancellationToken);
        var checkIns = await _db.CheckIns.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.Date)
            .ToListAsync(cancellationToken);
        var flags = await _db.Flags.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.FlaggedAt)
            .ToListAsync(cancellationToken);
        var assignedWork = await _db.AssignedWork.AsNoTracking()
            .Where(item => item.YouthUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        var messages = await _db.Messages.AsNoTracking()
            .Where(item => item.SenderId == userId || item.RecipientId == userId)
            .OrderBy(item => item.SentAt)
            .ToListAsync(cancellationToken);
        var notifications = await _db.Notifications.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var scheduledNotifications = await _db.ScheduledNotifications.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var notificationPreferences = await _db.NotificationPreferences.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.EventKey)
            .ToListAsync(cancellationToken);
        var displayPreference = await _db.DisplayPreferences.AsNoTracking()
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        var supportRequests = await _db.SupportRequests.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var storedUserFiles = await _db.StoredUserFiles.AsNoTracking()
            .Where(item => item.OwnerUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var dataSubjectRequests = await _db.DataSubjectRequests.AsNoTracking()
            .Where(item => item.SubjectUserId == userId)
            .OrderBy(item => item.RequestedAt)
            .ToListAsync(cancellationToken);
        var outboundDeliveryMetadata = await _db.OutboundEmails.AsNoTracking()
            .Where(item => item.SubjectUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new
            {
                item.Id,
                item.CreatedAt,
                item.AvailableAt,
                item.SentAt,
                DeliveryState = item.SentAt != null
                    ? "sent"
                    : item.LeaseUntil != null
                        ? "processing"
                        : item.LastAttemptAt != null
                            ? "retrying-or-dead-letter"
                            : "queued",
            })
            .ToListAsync(cancellationToken);

        var contactRequests = await _db.ContactRequests.AsNoTracking()
            .Where(item => item.OwnerUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var volunteerSignups = await _db.VolunteerSignups.AsNoTracking()
            .Where(item => item.OwnerUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var donationPledges = await _db.DonationPledges.AsNoTracking()
            .Where(item => item.OwnerUserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        // Staff actions are personal employment/activity records, but the child record and authored
        // free text are deliberately not copied into a staff member's export.
        var staffAttendanceActivity = await _db.AttendanceRecords.AsNoTracking()
            .Where(item => item.RecordedByUserId == userId && item.YouthUserId != userId)
            .OrderBy(item => item.RecordedAt)
            .Select(item => new { item.Id, item.SessionId, item.RecordedAt })
            .ToListAsync(cancellationToken);
        var staffQuizActivity = await _db.QuizAttempts.AsNoTracking()
            .Where(item => item.ApprovedByUserId == userId && item.YouthUserId != userId)
            .OrderBy(item => item.ApprovedAt)
            .Select(item => new { item.Id, item.QuizId, item.ApprovedAt })
            .ToListAsync(cancellationToken);
        var staffSubmissionActivity = await _db.Submissions.AsNoTracking()
            .Where(item => item.ReviewedByUserId == userId && item.YouthUserId != userId)
            .OrderBy(item => item.ReviewedAt)
            .Select(item => new { item.Id, item.AssignmentId, item.ReviewedAt })
            .ToListAsync(cancellationToken);
        var staffBadgeActivity = await _db.BadgeAwards.AsNoTracking()
            .Where(item => item.AwardedByUserId == userId && item.YouthUserId != userId)
            .OrderBy(item => item.AwardedAt)
            .Select(item => new { item.Id, item.BadgeId, item.AwardedAt })
            .ToListAsync(cancellationToken);
        var staffAssessmentActivity = await _db.CompetencyAssessments.AsNoTracking()
            .Where(item => item.AssessedByUserId == userId && item.YouthUserId != userId)
            .OrderBy(item => item.AssessedAt)
            .Select(item => new { item.Id, item.CompetencyId, item.AssessedAt })
            .ToListAsync(cancellationToken);
        var staffCheckInActivity = await _db.CheckIns.AsNoTracking()
            .Where(item => item.MentorUserId == userId && item.YouthUserId != userId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new { item.Id, item.Date, item.Type, item.CreatedAt })
            .ToListAsync(cancellationToken);
        var staffFlagActivity = await _db.Flags.AsNoTracking()
            .Where(item => item.FlaggedByUserId == userId && item.YouthUserId != userId)
            .OrderBy(item => item.FlaggedAt)
            .Select(item => new { item.Id, item.FlaggedAt, item.IsAutomatic, item.ResolvedAt })
            .ToListAsync(cancellationToken);
        var staffAssignedWorkActivity = await _db.AssignedWork.AsNoTracking()
            .Where(item => item.AssignedByUserId == userId && item.YouthUserId != userId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new
            {
                item.Id,
                item.ItemType,
                item.ItemId,
                item.DueDate,
                item.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var personalTargetIds = BuildPersonalTargetIds(
            userId,
            applications.Select(item => item.Id),
            guardianConsents.Select(item => item.Id),
            guardianConsentRequests.Select(item => item.Id),
            attendance.Select(item => item.Id),
            rsvps.Select(item => item.Id),
            lessonProgress.Select(item => item.Id),
            codingWorkspaces.Select(item => item.Id),
            readingStates.Select(item => item.Id),
            quizAttempts.Select(item => item.Id),
            submissions.Select(item => item.Id),
            reflections.Select(item => item.Id),
            assessments.Select(item => item.Id),
            checkIns.Select(item => item.Id),
            flags.Select(item => item.Id),
            assignedWork.Select(item => item.Id),
            supportRequests.Select(item => item.Id),
            dataSubjectRequests.Select(item => item.Id));
        var auditEntries = await LoadPersonalAuditAsync(userId, personalTargetIds, cancellationToken);

        var referencedSessionIds = attendance.Select(item => item.SessionId)
            .Concat(rsvps.Select(item => item.SessionId))
            .Distinct()
            .ToArray();
        var sessions = referencedSessionIds.Length == 0
            ? []
            : await _db.Sessions.AsNoTracking()
                .Where(item => referencedSessionIds.Contains(item.Id))
                .OrderBy(item => item.StartsAt)
                .Select(item => new
                {
                    item.Id,
                    item.Title,
                    item.StartsAt,
                    item.DurationMinutes,
                    item.Location,
                })
                .ToListAsync(cancellationToken);
        var lessonIds = lessonProgress.Select(item => item.LessonId)
            .Concat(codingWorkspaces.Select(item => item.LessonId))
            .Concat(readingStates.Select(item => item.LessonId))
            .Concat(reflections.Select(item => item.LessonId))
            .Distinct()
            .ToArray();
        var lessons = lessonIds.Length == 0
            ? []
            : await _db.Lessons.AsNoTracking()
                .Where(item => lessonIds.Contains(item.Id))
                .OrderBy(item => item.Title)
                .Select(item => new { item.Id, item.Title })
                .ToListAsync(cancellationToken);
        var assignmentIds = submissions.Select(item => item.AssignmentId)
            .Concat(submissionFiles.Select(item => item.AssignmentId))
            .Distinct()
            .ToArray();
        var assignments = assignmentIds.Length == 0
            ? []
            : await _db.Assignments.AsNoTracking()
                .Where(item => assignmentIds.Contains(item.Id))
                .OrderBy(item => item.Title)
                .Select(item => new { item.Id, item.Title })
                .ToListAsync(cancellationToken);
        var trackIds = trackEnrollments.Select(item => item.TrackId).Distinct().ToArray();
        var tracks = trackIds.Length == 0
            ? []
            : await _db.Tracks.AsNoTracking()
                .Where(item => trackIds.Contains(item.Id))
                .OrderBy(item => item.Name)
                .Select(item => new { item.Id, item.Name, item.Key })
                .ToListAsync(cancellationToken);
        var badgeIds = badgeAwards.Select(item => item.BadgeId).Distinct().ToArray();
        var badges = badgeIds.Length == 0
            ? []
            : await _db.Badges.AsNoTracking()
                .Where(item => badgeIds.Contains(item.Id))
                .OrderBy(item => item.Name)
                .Select(item => new { item.Id, item.Name, item.Description })
                .ToListAsync(cancellationToken);
        var competencyIds = assessments.Select(item => item.CompetencyId).Distinct().ToArray();
        var competencies = competencyIds.Length == 0
            ? []
            : await _db.Competencies.AsNoTracking()
                .Where(item => competencyIds.Contains(item.Id))
                .OrderBy(item => item.Name)
                .Select(item => new { item.Id, item.Name })
                .ToListAsync(cancellationToken);

        var document = new
        {
            schemaVersion = "1.0",
            generatedAt,
            accessGrant = new
            {
                accessGrant.Reference,
                accessGrant.RequestedAt,
                accessGrant.CompletedAt,
            },
            account = new
            {
                user.Id,
                user.DisplayName,
                user.Email,
                user.Role,
                status = user.Status.ToString(),
                user.Pronouns,
                user.PhotoUrl,
                user.OnboardingCompleted,
                user.CohortId,
                user.CreatedAt,
                user.LastSignInAt,
                user.DeactivatedAt,
                user.PersonalDataAnonymizedAt,
                guardianProfile = guardianProfile is null
                    ? null
                    : new
                    {
                        guardianProfile.Id,
                        guardianProfile.Relationship,
                        guardianProfile.Phone,
                        guardianProfile.EmergencyContactName,
                        guardianProfile.EmergencyContactPhone,
                        guardianProfile.UpdatedAt,
                    },
            },
            applicationAndConsent = new
            {
                applications = applications.Select(item =>
                {
                    var isApplicant = item.CreatedYouthUserId == userId
                        || item.ApplicantUserId == userId;
                    return new
                    {
                        item.Id,
                        perspective = isApplicant ? "applicant" : "guardian-contact",
                        firstName = isApplicant ? item.FirstName : null,
                        lastName = isApplicant ? item.LastName : null,
                        age = isApplicant ? item.Age : (int?)null,
                        whoIsFilling = isApplicant ? item.WhoIsFilling : null,
                        neighbourhood = isApplicant ? item.Neighbourhood : null,
                        cohortInterest = isApplicant ? item.CohortInterest : null,
                        whyInterested = isApplicant ? item.WhyInterested : null,
                        trackInterest = isApplicant ? item.TrackInterest : null,
                        guardianName = isApplicant ? null : item.GuardianName,
                        guardianEmail = isApplicant ? null : item.GuardianEmail,
                        guardianPhone = isApplicant ? null : item.GuardianPhone,
                        participationConsentAck = isApplicant
                            ? item.ParticipationConsentAck
                            : (bool?)null,
                        privacyAck = isApplicant ? item.PrivacyAck : (bool?)null,
                        status = isApplicant ? item.Status.ToString() : null,
                        decisionNote = isApplicant ? item.DecisionNote : null,
                        cohortId = isApplicant ? item.CohortId : null,
                        item.CreatedAt,
                        reviewedAt = isApplicant ? item.ReviewedAt : null,
                    };
                }),
                guardianConsents = guardianConsents.Select(item =>
                {
                    var isGuardian = item.GuardianUserId == userId;
                    return new
                    {
                        item.Id,
                        perspective = isGuardian ? "guardian" : "youth",
                        guardianName = isGuardian ? item.GuardianName : null,
                        relationship = isGuardian ? item.Relationship : null,
                        item.ParticipationConsent,
                        item.MediaConsent,
                        item.MediaAllPhotosVideo,
                        item.MediaGroupOnly,
                        item.MediaFirstNameCaptions,
                        signature = isGuardian ? item.Signature : null,
                        item.Method,
                        item.ReferenceId,
                        item.TermId,
                        item.Term,
                        item.SignedAt,
                        item.ValidFrom,
                        item.ValidUntil,
                        item.WithdrawnAt,
                    };
                }),
                guardianConsentRequests = guardianConsentRequests.Select(item => new
                {
                    item.Id,
                    perspective = item.GuardianUserId == userId ? "guardian" : "youth",
                    item.Channel,
                    destination = item.GuardianUserId == userId ? item.Destination : null,
                    item.CreatedAt,
                    item.ExpiresAt,
                    item.UsedAt,
                }),
            },
            attendanceAndEngagement = new
            {
                attendance = attendance.Select(item => new
                {
                    item.Id,
                    item.SessionId,
                    status = item.Status.ToString(),
                    item.Note,
                    recordedBy = "staff-identity-redacted",
                    item.RecordedAt,
                }),
                attendanceWriteReceipts = attendanceReceipts.Select(item => new
                {
                    item.Id,
                    item.Present,
                    item.Late,
                    item.Excused,
                    item.Absent,
                    item.Marked,
                    item.Total,
                    item.CreatedAt,
                }),
                rsvps = rsvps.Select(item => new
                {
                    item.Id,
                    item.SessionId,
                    response = item.Response.ToString(),
                    item.Note,
                    item.ReminderSet,
                    item.CreatedAt,
                    item.UpdatedAt,
                }),
                announcementReads = announcementReads.Select(item => new
                {
                    item.Id,
                    item.AnnouncementId,
                    item.ReadAt,
                }),
            },
            learning = new
            {
                lessonProgress = lessonProgress.Select(item => new
                {
                    item.Id,
                    item.LessonId,
                    status = item.Status.ToString(),
                    item.CompletedAt,
                    item.UpdatedAt,
                }),
                codingWorkspaces = codingWorkspaces.Select(item => new
                {
                    item.Id,
                    item.LessonId,
                    item.Code,
                    item.CheckedStepIdsJson,
                    item.CompletedAt,
                    item.UpdatedAt,
                }),
                trackEnrollments = trackEnrollments.Select(item => new
                {
                    item.Id,
                    item.TrackId,
                    item.EnrolledAt,
                }),
                readingStates = readingStates.Select(item => new
                {
                    item.Id,
                    item.LessonId,
                    item.TextSize,
                    item.Bookmarked,
                    item.Position,
                    item.UpdatedAt,
                }),
                quizAttempts = quizAttempts.Select(item => new
                {
                    item.Id,
                    item.QuizId,
                    item.AttemptNumber,
                    item.AnswersJson,
                    item.CurrentQuestionIndex,
                    item.AutoScore,
                    item.Passed,
                    item.IsSubmitted,
                    item.SubmittedAt,
                    item.ApprovedScore,
                    approvedBy = item.ApprovedByUserId.HasValue ? "staff-identity-redacted" : null,
                    item.ApprovedAt,
                    item.CreatedAt,
                }),
                submissions = submissions.Select(item => new
                {
                    item.Id,
                    item.AssignmentId,
                    item.BodyText,
                    item.PhotoUploadId,
                    item.PhotoUrl,
                    item.FileUploadId,
                    item.LinkUrl,
                    item.IsCodingProject,
                    item.YouthNote,
                    status = item.Status.ToString(),
                    item.MentorComment,
                    item.RubricScoresJson,
                    item.TotalAwarded,
                    item.GuardianNotified,
                    reviewedBy = item.ReviewedByUserId.HasValue ? "staff-identity-redacted" : null,
                    item.ReviewedAt,
                    item.SubmittedAt,
                    item.CreatedAt,
                }),
                submissionFiles = submissionFiles.Select(item => new
                {
                    item.Id,
                    item.AssignmentId,
                    kind = item.Kind.ToString(),
                    item.OriginalFileName,
                    item.ContentType,
                    item.SizeBytes,
                    item.Sha256,
                    contentBase64 = Convert.ToBase64String(item.Content),
                    item.CreatedAt,
                }),
                submissionTimeline = submissionTimeline.Select(item => new
                {
                    item.Id,
                    item.SubmissionId,
                    item.Type,
                    item.OccurredAt,
                }),
                reflections = reflections.Select(item => new
                {
                    item.Id,
                    item.LessonId,
                    item.Text,
                    item.Mood,
                    item.SecondaryJson,
                    visibility = item.Visibility.ToString(),
                    item.IsDraft,
                    item.CreatedAt,
                    item.UpdatedAt,
                }),
                badgeAwards = badgeAwards.Select(item => new
                {
                    item.Id,
                    item.BadgeId,
                    awardedBy = item.AwardedByUserId.HasValue ? "staff-identity-redacted" : "automatic",
                    item.AwardedAt,
                }),
            },
            mentoring = new
            {
                competencyAssessments = assessments.Select(item => new
                {
                    item.Id,
                    item.CompetencyId,
                    level = item.Level.ToString(),
                    item.Notes,
                    assessedBy = "staff-identity-redacted",
                    item.AssessedAt,
                }),
                checkIns = checkIns.Select(item => new
                {
                    item.Id,
                    item.Date,
                    item.Type,
                    item.Engagement,
                    item.GoalsDiscussed,
                    prayerRequests = item.Visibility == Visibility.Private
                        ? "redacted-private-staff-note"
                        : item.PrayerRequests,
                    item.ActionItems,
                    item.NextFollowUp,
                    visibility = item.Visibility.ToString(),
                    item.GuardianInformed,
                    mentor = "staff-identity-redacted",
                    item.CreatedAt,
                }),
                flags = flags.Select(item => new
                {
                    item.Id,
                    item.Reason,
                    flaggedBy = "staff-identity-redacted",
                    item.FlaggedAt,
                    item.IsAutomatic,
                    item.AutomaticReason,
                    item.AutomaticSignalKeys,
                    item.AutomaticSignalsClearedAt,
                    item.ResolvedAt,
                    item.ResolutionReason,
                }),
                assignedWork = assignedWork.Select(item => new
                {
                    item.Id,
                    item.ItemType,
                    item.ItemId,
                    item.CohortId,
                    item.DueDate,
                    item.ReminderCadence,
                    item.Message,
                    assignedBy = "staff-identity-redacted",
                    item.CreatedAt,
                }),
            },
            communicationsAndPreferences = new
            {
                messages = messages.Select(item => new
                {
                    item.Id,
                    direction = item.SenderId == userId ? "sent" : "received",
                    otherParticipant = "identity-redacted",
                    item.Body,
                    item.SentAt,
                    item.ReadAt,
                }),
                notifications = notifications.Select(item => new
                {
                    item.Id,
                    item.EventKey,
                    item.Title,
                    item.Body,
                    item.LinkPath,
                    item.ReadAt,
                    item.CreatedAt,
                }),
                scheduledNotifications = scheduledNotifications.Select(item => new
                {
                    item.Id,
                    item.EventKey,
                    item.Title,
                    item.Body,
                    item.LinkPath,
                    item.SourceType,
                    item.SourceId,
                    item.DeliverAt,
                    item.DeliveredAt,
                    item.CancelledAt,
                    item.CreatedAt,
                }),
                notificationPreferences = notificationPreferences.Select(item => new
                {
                    item.Id,
                    item.EventKey,
                    item.InApp,
                    item.Email,
                }),
                displayPreference = displayPreference is null
                    ? null
                    : new
                    {
                        displayPreference.Id,
                        displayPreference.TextScalePercent,
                        displayPreference.HighContrast,
                        displayPreference.ReduceMotion,
                        displayPreference.UnderlineLinks,
                        displayPreference.ReadingFont,
                        displayPreference.Theme,
                        displayPreference.UpdatedAt,
                    },
                supportRequests = supportRequests.Select(item => new
                {
                    item.Id,
                    item.Category,
                    item.Description,
                    item.ReplyToEmail,
                    item.ScreenshotFileId,
                    item.CreatedAt,
                }),
                userFiles = storedUserFiles.Select(item => new
                {
                    item.Id,
                    purpose = item.Purpose.ToString(),
                    item.OriginalFileName,
                    item.ContentType,
                    item.SizeBytes,
                    item.Sha256,
                    contentBase64 = Convert.ToBase64String(item.Content),
                    item.CreatedAt,
                }),
            },
            privacyAndAudit = new
            {
                dataSubjectRequests = dataSubjectRequests.Select(item => new
                {
                    item.Id,
                    item.Reference,
                    item.ActionType,
                    item.Details,
                    requestedCorrection = item.ActionType == "correction"
                        ? new
                        {
                            item.RequestedDisplayName,
                            item.RequestedEmail,
                            item.RequestedPronouns,
                            item.RequestedClearPronouns,
                        }
                        : null,
                    status = item.Status.ToString(),
                    item.RequestedAt,
                    item.CompletedAt,
                    item.ResolutionNote,
                }),
                auditEntries = auditEntries.Select(item => new
                {
                    item.Id,
                    relation = item.ActorUserId == userId ? "actor" : "subject-record",
                    item.Action,
                    item.TargetType,
                    item.TargetId,
                    item.At,
                    source = item.ActorUserId == userId ? item.Source : null,
                    item.IsPrivate,
                    details = (string?)null,
                }),
            },
            publicSubmissionScope = new
            {
                included = PrivacyScopeNotices.AccountLinkedPublicSubmissions,
                unlinkedHandling = PrivacyScopeNotices.UnlinkedPublicSubmissions,
            },
            accountLinkedPublicSubmissions = new
            {
                contactRequests = contactRequests.Select(item => new
                {
                    item.Id,
                    item.Name,
                    item.Email,
                    item.Topic,
                    item.Message,
                    item.CreatedAt,
                }),
                volunteerSignups = volunteerSignups.Select(item => new
                {
                    item.Id,
                    item.Name,
                    item.Email,
                    item.Phone,
                    item.AreasOfInterest,
                    item.ScreeningConsentAck,
                    item.CreatedAt,
                }),
                donationPledges = donationPledges.Select(item => new
                {
                    item.Id,
                    item.DonorName,
                    item.Email,
                    item.Tier,
                    item.MonthlyAmount,
                    item.Frequency,
                    item.ProviderReference,
                    item.CreatedAt,
                }),
            },
            outboundDeliveryMetadata = outboundDeliveryMetadata.Select(item => new
            {
                item.Id,
                purpose = "transactional-notification",
                item.CreatedAt,
                item.AvailableAt,
                item.SentAt,
                deliveryState = item.DeliveryState,
            }),
            staffActivity = new
            {
                attendanceMarks = staffAttendanceActivity,
                quizApprovals = staffQuizActivity,
                submissionReviews = staffSubmissionActivity,
                badgeAwards = staffBadgeActivity,
                competencyAssessments = staffAssessmentActivity,
                checkIns = staffCheckInActivity,
                flags = staffFlagActivity,
                assignedWork = staffAssignedWorkActivity,
            },
            catalogReferences = new
            {
                sessions,
                lessons,
                assignments,
                tracks,
                badges,
                competencies,
            },
            exclusions = new[]
            {
                "password hashes, session versions, lockout counters, and authentication tokens",
                "invitation, password-reset, consent-capability, and refresh-token secrets",
                "outbound recipient addresses, delivery secrets, and cross-recipient payload copies; only safe delivery metadata is included",
                "third-party user identifiers and staff identities",
                "private staff prayer-request notes and audit free text",
                "attendance idempotency keys and request fingerprints",
                "database concurrency and integrity implementation fields",
            },
        };

        var content = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        await _audit.LogAsync(
            "PersonalDataExportDownloaded",
            userId,
            "User",
            userId,
            details: $"accessRequest={accessGrant.Reference}",
            cancellationToken: cancellationToken);

        return new PersonalDataExportResult(
            $"waypoint-personal-data-{userId:N}-{generatedAt:yyyyMMdd}.json",
            content);
    }

    private static HashSet<Guid> BuildPersonalTargetIds(
        Guid userId,
        params IEnumerable<Guid>[] idSets)
    {
        var ids = new HashSet<Guid> { userId };
        foreach (var idSet in idSets)
        {
            ids.UnionWith(idSet);
        }
        return ids;
    }

    private async Task<List<AuditEntry>> LoadPersonalAuditAsync(
        Guid userId,
        IReadOnlyCollection<Guid> targetIds,
        CancellationToken cancellationToken)
    {
        var entries = await _db.AuditEntries.AsNoTracking()
            .Where(item => item.ActorUserId == userId || item.TargetId == userId)
            .ToListAsync(cancellationToken);
        var seen = entries.Select(item => item.Id).ToHashSet();

        // Chunked predicates avoid SQL Server's parameter limit for long-lived accounts.
        foreach (var targetIdChunk in targetIds.Where(item => item != userId).Chunk(400))
        {
            var chunk = targetIdChunk.ToArray();
            var related = await _db.AuditEntries.AsNoTracking()
                .Where(item => item.TargetId.HasValue && chunk.Contains(item.TargetId.Value))
                .ToListAsync(cancellationToken);
            foreach (var entry in related)
            {
                if (seen.Add(entry.Id)) entries.Add(entry);
            }
        }

        return entries.OrderBy(item => item.At).ToList();
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

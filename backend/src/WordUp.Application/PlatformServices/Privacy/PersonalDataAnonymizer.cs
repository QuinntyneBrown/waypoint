using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.PlatformServices.Privacy;

/// <summary>
/// Removes directly identifying and user-authored private content while retaining pseudonymous keys,
/// dates, statuses, scores, attendance, and progress needed for de-identified reporting.
/// </summary>
public sealed class PersonalDataAnonymizer
{
    private const string Removed = "[removed]";
    private readonly IAppDbContext _db;

    public PersonalDataAnonymizer(IAppDbContext db) => _db = db;

    public async Task AnonymizeAsync(User user, DateTime now, CancellationToken cancellationToken)
    {
        if (user.PersonalDataAnonymizedAt is not null)
        {
            return;
        }

        var oldEmail = user.Email;
        user.Email = $"removed+{user.Id:N}@anonymized.invalid";
        user.DisplayName = "Former participant";
        user.PasswordHash = Removed;
        user.Status = UserStatus.Removed;
        user.SessionVersion++;
        user.Pronouns = null;
        user.PhotoUrl = null;
        user.OnboardingCompleted = false;
        user.GuardianUserId = null;
        user.FailedSignInAttempts = 0;
        user.LockoutEndsAt = null;
        user.DeactivatedAt ??= now;
        user.PersonalDataAnonymizedAt = now;

        var refreshTokens = await _db.RefreshTokens
            .Where(item => item.UserId == user.Id && item.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in refreshTokens) token.RevokedAt = now;

        var resetTokens = await _db.PasswordResetTokens
            .Where(item => item.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.PasswordResetTokens.RemoveRange(resetTokens);

        var privacyRequests = await _db.DataSubjectRequests
            .Where(item => item.SubjectUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in privacyRequests)
        {
            // Preserve the durable right-request ledger while removing subject-authored and
            // administrator-authored personal free text and requested correction values.
            item.Details = null;
            item.RequestedDisplayName = null;
            item.RequestedEmail = null;
            item.RequestedPronouns = null;
            item.RequestedClearPronouns = false;
            item.ResolutionNote = null;
        }

        // Recipient and content-subject links are distinct: remove both without inferring ownership
        // from an encrypted address that another person might share or later acquire.
        var personalEmails = await _db.OutboundEmails
            .Where(item => item.SubjectUserId == user.Id || item.RecipientUserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.OutboundEmails.RemoveRange(personalEmails);

        var privateFiles = await _db.StoredUserFiles
            .Where(item => item.OwnerUserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.StoredUserFiles.RemoveRange(privateFiles);
        var submissionFiles = await _db.StoredSubmissionFiles
            .Where(item => item.OwnerUserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.StoredSubmissionFiles.RemoveRange(submissionFiles);

        var checkIns = await _db.CheckIns
            .Where(item => item.YouthUserId == user.Id || item.MentorUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in checkIns.Where(item => item.YouthUserId == user.Id))
        {
            item.Engagement = null;
            item.GoalsDiscussed = null;
            item.PrayerRequests = null;
            item.ActionItems = null;
        }

        var messages = await _db.Messages
            .Where(item => item.SenderId == user.Id || item.RecipientId == user.Id)
            .ToListAsync(cancellationToken);
        if (user.Role is Roles.Youth or Roles.Guardian)
        {
            foreach (var item in messages) item.Body = Removed;
        }

        var attendance = await _db.AttendanceRecords
            .Where(item => item.YouthUserId == user.Id || item.RecordedByUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in attendance.Where(item => item.YouthUserId == user.Id)) item.Note = null;

        var reflections = await _db.Reflections
            .Where(item => item.YouthUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in reflections)
        {
            item.Text = null;
            item.Mood = null;
            item.SecondaryJson = null;
        }

        var quizAttempts = await _db.QuizAttempts
            .Where(item => item.YouthUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in quizAttempts) item.AnswersJson = null;

        var workspaces = await _db.CodingLessonWorkspaces
            .Where(item => item.YouthUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in workspaces)
        {
            item.Code = string.Empty;
            item.CheckedStepIdsJson = "[]";
        }

        var submissions = await _db.Submissions
            .Where(item => item.YouthUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in submissions)
        {
            item.BodyText = null;
            item.PhotoUploadId = null;
            item.PhotoUrl = null;
            item.FileUploadId = null;
            item.LinkUrl = null;
            item.YouthNote = null;
            item.MentorComment = null;
        }

        var rsvps = await _db.SessionRsvps
            .Where(item => item.YouthUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in rsvps) item.Note = null;

        var assessments = await _db.CompetencyAssessments
            .Where(item => item.YouthUserId == user.Id || item.AssessedByUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in assessments.Where(item => item.YouthUserId == user.Id)) item.Notes = null;

        var flags = await _db.Flags
            .Where(item => item.YouthUserId == user.Id || item.FlaggedByUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in flags.Where(item => item.YouthUserId == user.Id))
        {
            item.Reason = Removed;
            item.AutomaticReason = null;
            item.AutomaticSignalKeys = null;
            item.ResolutionReason = null;
        }

        var assignedWork = await _db.AssignedWork
            .Where(item => item.YouthUserId == user.Id || item.AssignedByUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in assignedWork.Where(item => item.YouthUserId == user.Id)) item.Message = null;

        var supportRequests = await _db.SupportRequests
            .Where(item => item.UserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in supportRequests)
        {
            item.Description = Removed;
            item.ReplyToEmail = user.Email;
            item.ScreenshotFileId = null;
        }

        var guardianProfiles = await _db.GuardianProfiles
            .Where(item => item.GuardianUserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.GuardianProfiles.RemoveRange(guardianProfiles);

        var consentRequests = await _db.GuardianConsentRequests
            .Where(item => item.YouthUserId == user.Id || item.GuardianUserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.GuardianConsentRequests.RemoveRange(consentRequests);
        var consents = await _db.GuardianConsents
            .Where(item => item.YouthUserId == user.Id || item.GuardianUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in consents)
        {
            item.GuardianName = Removed;
            item.Relationship = Removed;
            item.Signature = Removed;
            item.GuardianUserId = item.GuardianUserId == user.Id ? null : item.GuardianUserId;
            if (item.YouthUserId == user.Id)
            {
                item.WithdrawnAt ??= now;
            }
        }

        var applications = await _db.Applications
            .Where(item => item.CreatedYouthUserId == user.Id
                || item.ApplicantUserId == user.Id
                || item.GuardianUserId == user.Id)
            .ToListAsync(cancellationToken);
        foreach (var item in applications)
        {
            if (item.CreatedYouthUserId == user.Id || item.ApplicantUserId == user.Id)
            {
                item.FirstName = "Former";
                item.LastName = "Participant";
                item.WhoIsFilling = Removed;
                item.Neighbourhood = Removed;
                item.CohortInterest = Removed;
                item.WhyInterested = Removed;
                item.TrackInterest = null;
                item.DecisionNote = null;
            }
            if (item.GuardianUserId == user.Id)
            {
                item.GuardianName = null;
                item.GuardianEmail = null;
                item.GuardianPhone = null;
            }
        }

        var invitations = await _db.Invitations
            .Where(item => item.Email == oldEmail)
            .ToListAsync(cancellationToken);
        foreach (var item in invitations)
        {
            item.Email = $"removed+{item.Id:N}@anonymized.invalid";
            item.TokenHash = Removed;
        }

        var notifications = await _db.Notifications
            .Where(item => item.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.Notifications.RemoveRange(notifications);
        var scheduled = await _db.ScheduledNotifications
            .Where(item => item.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.ScheduledNotifications.RemoveRange(scheduled);
        var notificationPreferences = await _db.NotificationPreferences
            .Where(item => item.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.NotificationPreferences.RemoveRange(notificationPreferences);
        var displayPreferences = await _db.DisplayPreferences
            .Where(item => item.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.DisplayPreferences.RemoveRange(displayPreferences);
        var attendanceReceipts = await _db.AttendanceWriteReceipts
            .Where(item => item.OwnerUserId == user.Id)
            .ToListAsync(cancellationToken);
        _db.AttendanceWriteReceipts.RemoveRange(attendanceReceipts);

        await AnonymizePublicSubmissionsAsync(user.Id, user.Email, cancellationToken);
    }

    private async Task AnonymizePublicSubmissionsAsync(
        Guid ownerUserId,
        string anonymizedEmail,
        CancellationToken cancellationToken)
    {
        // Anonymous submissions are not account-owned merely because their address matches. Only an
        // authenticated ownership link authorizes this automated account-rights workflow.
        var contacts = await _db.ContactRequests
            .Where(item => item.OwnerUserId == ownerUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in contacts)
        {
            item.Name = "Former participant";
            item.Email = anonymizedEmail;
            item.Topic = Removed;
            item.Message = Removed;
        }

        var volunteers = await _db.VolunteerSignups
            .Where(item => item.OwnerUserId == ownerUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in volunteers)
        {
            item.Name = "Former participant";
            item.Email = anonymizedEmail;
            item.Phone = null;
            item.AreasOfInterest = null;
        }

        var donations = await _db.DonationPledges
            .Where(item => item.OwnerUserId == ownerUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in donations)
        {
            item.DonorName = "Former participant";
            item.Email = anonymizedEmail;
            item.ProviderReference = null;
        }
    }

}

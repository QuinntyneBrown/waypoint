using Microsoft.EntityFrameworkCore;

namespace WordUp.Application.Shared.Abstractions;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Invitation> Invitations { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    DbSet<YouthApplication> Applications { get; }
    DbSet<GuardianConsent> GuardianConsents { get; }
    DbSet<GuardianConsentRequest> GuardianConsentRequests { get; }
    DbSet<Cohort> Cohorts { get; }
    DbSet<CohortMentor> CohortMentors { get; }

    // Curriculum authoring (M2)
    DbSet<Track> Tracks { get; }
    DbSet<Module> Modules { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<CodingLessonStep> CodingLessonSteps { get; }
    DbSet<LessonMedia> LessonMedia { get; }
    DbSet<Quiz> Quizzes { get; }
    DbSet<QuizQuestion> QuizQuestions { get; }
    DbSet<Assignment> Assignments { get; }
    DbSet<RubricCriterion> RubricCriteria { get; }
    DbSet<Badge> Badges { get; }
    DbSet<BadgeAward> BadgeAwards { get; }
    DbSet<ProgressionStage> ProgressionStages { get; }
    DbSet<Competency> Competencies { get; }

    // Program administration (M2)
    DbSet<OrgSettings> OrgSettings { get; }
    DbSet<Term> Terms { get; }

    // Learning & mentoring (M3)
    DbSet<LessonProgress> LessonProgress { get; }
    DbSet<CodingLessonWorkspace> CodingLessonWorkspaces { get; }
    DbSet<YouthTrackEnrollment> YouthTrackEnrollments { get; }
    DbSet<LessonReadingState> LessonReadingStates { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }
    DbSet<Submission> Submissions { get; }
    DbSet<StoredSubmissionFile> StoredSubmissionFiles { get; }
    DbSet<SubmissionTimelineEvent> SubmissionTimelineEvents { get; }
    DbSet<ReflectionEntry> Reflections { get; }
    DbSet<CompetencyAssessment> CompetencyAssessments { get; }
    DbSet<CheckIn> CheckIns { get; }
    DbSet<Flag> Flags { get; }
    DbSet<AssignedWork> AssignedWork { get; }

    // Engagement & operations (M4)
    DbSet<Session> Sessions { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<AttendanceWriteReceipt> AttendanceWriteReceipts { get; }
    DbSet<SessionRsvp> SessionRsvps { get; }
    DbSet<Announcement> Announcements { get; }
    DbSet<AnnouncementRead> AnnouncementReads { get; }
    DbSet<Message> Messages { get; }
    DbSet<Notification> Notifications { get; }

    // Platform services (M4)
    DbSet<NotificationPreference> NotificationPreferences { get; }
    DbSet<DisplayPreference> DisplayPreferences { get; }
    DbSet<SupportRequest> SupportRequests { get; }

    // Guardian portal + public site (M5)
    DbSet<GuardianProfile> GuardianProfiles { get; }
    DbSet<ContactRequest> ContactRequests { get; }
    DbSet<VolunteerSignup> VolunteerSignups { get; }
    DbSet<DonationPledge> DonationPledges { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

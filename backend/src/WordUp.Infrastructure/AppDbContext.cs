using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<YouthApplication> Applications => Set<YouthApplication>();
    public DbSet<GuardianConsent> GuardianConsents => Set<GuardianConsent>();
    public DbSet<Cohort> Cohorts => Set<Cohort>();
    public DbSet<CohortMentor> CohortMentors => Set<CohortMentor>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonMedia> LessonMedia => Set<LessonMedia>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<RubricCriterion> RubricCriteria => Set<RubricCriterion>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<BadgeAward> BadgeAwards => Set<BadgeAward>();
    public DbSet<ProgressionStage> ProgressionStages => Set<ProgressionStage>();
    public DbSet<Competency> Competencies => Set<Competency>();
    public DbSet<OrgSettings> OrgSettings => Set<OrgSettings>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
    public DbSet<LessonReadingState> LessonReadingStates => Set<LessonReadingState>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<ReflectionEntry> Reflections => Set<ReflectionEntry>();
    public DbSet<CompetencyAssessment> CompetencyAssessments => Set<CompetencyAssessment>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<Flag> Flags => Set<Flag>();
    public DbSet<AssignedWork> AssignedWork => Set<AssignedWork>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<SessionRsvp> SessionRsvps => Set<SessionRsvp>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementRead> AnnouncementReads => Set<AnnouncementRead>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<DisplayPreference> DisplayPreferences => Set<DisplayPreference>();
    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();
    public DbSet<GuardianProfile> GuardianProfiles => Set<GuardianProfile>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<VolunteerSignup> VolunteerSignups => Set<VolunteerSignup>();
    public DbSet<DonationPledge> DonationPledges => Set<DonationPledge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
            b.Property(x => x.Role).HasMaxLength(64).IsRequired();
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.Pronouns).HasMaxLength(64);
            b.Property(x => x.PhotoUrl).HasMaxLength(512);
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.TokenHash);
            b.HasIndex(x => x.FamilyId);
            b.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
            b.Property(x => x.ReplacedByTokenHash).HasMaxLength(512);
        });

        modelBuilder.Entity<Invitation>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.TokenHash);
            b.HasIndex(x => x.Email);
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.Role).HasMaxLength(64).IsRequired();
            b.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
            b.Property(x => x.Status).HasConversion<int>();
        });

        modelBuilder.Entity<PasswordResetToken>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.TokenHash);
            b.HasIndex(x => x.UserId);
            b.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
        });

        modelBuilder.Entity<AuditEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.At);
            b.HasIndex(x => x.ActorUserId);
            b.Property(x => x.Action).HasMaxLength(128).IsRequired();
            b.Property(x => x.TargetType).HasMaxLength(128);
            b.Property(x => x.Source).HasMaxLength(512);
            b.Property(x => x.Details).HasMaxLength(2048);
        });

        modelBuilder.Entity<YouthApplication>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Status);
            b.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(128).IsRequired();
            b.Property(x => x.Neighbourhood).HasMaxLength(128).IsRequired();
            b.Property(x => x.WhyInterested).HasMaxLength(2000).IsRequired();
            b.Property(x => x.TrackInterest).HasMaxLength(64);
            b.Property(x => x.GuardianName).HasMaxLength(128);
            b.Property(x => x.GuardianEmail).HasMaxLength(256);
            b.Property(x => x.GuardianPhone).HasMaxLength(64);
            b.Property(x => x.DecisionNote).HasMaxLength(2000);
            b.Property(x => x.Status).HasConversion<int>();
        });

        modelBuilder.Entity<GuardianConsent>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.YouthUserId);
            b.Property(x => x.GuardianName).HasMaxLength(128).IsRequired();
            b.Property(x => x.Relationship).HasMaxLength(64).IsRequired();
            b.Property(x => x.Signature).HasMaxLength(128).IsRequired();
            b.Property(x => x.Method).HasMaxLength(64).IsRequired();
            b.Property(x => x.ReferenceId).HasMaxLength(64).IsRequired();
            b.Property(x => x.Term).HasMaxLength(64);
        });

        modelBuilder.Entity<Cohort>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.Location).HasMaxLength(256);
        });

        // Curriculum authoring (M2)
        modelBuilder.Entity<Track>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.Key).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Key).IsUnique();
        });
        modelBuilder.Entity<Module>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.HasIndex(x => x.TrackId);
        });
        modelBuilder.Entity<Lesson>(b =>
        {
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.ModuleId);
        });
        modelBuilder.Entity<LessonMedia>(b => b.HasIndex(x => x.LessonId));
        modelBuilder.Entity<Quiz>(b => b.Property(x => x.FeedbackMode).HasMaxLength(32));
        modelBuilder.Entity<QuizQuestion>(b =>
        {
            b.Property(x => x.Text).HasMaxLength(1000).IsRequired();
            b.HasIndex(x => x.QuizId);
        });
        modelBuilder.Entity<Assignment>(b =>
        {
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.SubmissionTypes).HasMaxLength(128);
        });
        modelBuilder.Entity<RubricCriterion>(b =>
        {
            b.Property(x => x.Description).HasMaxLength(500).IsRequired();
            b.HasIndex(x => x.AssignmentId);
        });
        modelBuilder.Entity<Badge>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.Icon).HasMaxLength(64).IsRequired();
        });
        modelBuilder.Entity<BadgeAward>(b => b.HasIndex(x => new { x.BadgeId, x.YouthUserId }));
        modelBuilder.Entity<ProgressionStage>(b => b.Property(x => x.Name).HasMaxLength(128).IsRequired());
        modelBuilder.Entity<Competency>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.HasIndex(x => x.StageId);
        });

        // Program administration (M2)
        modelBuilder.Entity<OrgSettings>(b => b.Property(x => x.Name).HasMaxLength(128).IsRequired());
        modelBuilder.Entity<Term>(b => b.Property(x => x.Name).HasMaxLength(128).IsRequired());
        modelBuilder.Entity<CohortMentor>(b => b.HasIndex(x => new { x.CohortId, x.MentorId }));

        // Learning & mentoring (M3)
        modelBuilder.Entity<LessonProgress>(b => b.HasIndex(x => new { x.YouthUserId, x.LessonId }).IsUnique());
        modelBuilder.Entity<LessonReadingState>(b => b.HasIndex(x => new { x.YouthUserId, x.LessonId }).IsUnique());
        modelBuilder.Entity<QuizAttempt>(b => b.HasIndex(x => new { x.YouthUserId, x.QuizId }));
        modelBuilder.Entity<Submission>(b => b.HasIndex(x => new { x.YouthUserId, x.AssignmentId }));
        modelBuilder.Entity<ReflectionEntry>(b => b.HasIndex(x => new { x.YouthUserId, x.LessonId }));
        modelBuilder.Entity<CompetencyAssessment>(b => b.HasIndex(x => new { x.YouthUserId, x.CompetencyId }));
        modelBuilder.Entity<CheckIn>(b =>
        {
            b.HasIndex(x => x.YouthUserId);
            b.Property(x => x.Type).HasMaxLength(32).IsRequired();
        });
        modelBuilder.Entity<Flag>(b =>
        {
            b.HasIndex(x => x.YouthUserId);
            b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            b.Property(x => x.ResolutionReason).HasMaxLength(128);
        });
        modelBuilder.Entity<AssignedWork>(b =>
        {
            b.HasIndex(x => x.YouthUserId);
            b.Property(x => x.ItemType).HasMaxLength(32).IsRequired();
        });

        // Engagement & operations (M4)
        modelBuilder.Entity<Session>(b =>
        {
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Location).HasMaxLength(256);
            b.HasIndex(x => x.CohortId);
            b.HasIndex(x => x.StartsAt);
            b.HasIndex(x => x.RecurrenceGroupId);
        });
        modelBuilder.Entity<AttendanceRecord>(b =>
        {
            b.HasIndex(x => new { x.SessionId, x.YouthUserId }).IsUnique();
            b.Property(x => x.Note).HasMaxLength(1000);
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.RowVersion).IsRowVersion();
        });
        modelBuilder.Entity<SessionRsvp>(b =>
        {
            b.HasIndex(x => new { x.SessionId, x.YouthUserId }).IsUnique();
            b.Property(x => x.Response).HasConversion<int>();
            b.Property(x => x.Note).HasMaxLength(1000);
        });
        modelBuilder.Entity<Announcement>(b =>
        {
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Message).HasMaxLength(4000).IsRequired();
            b.Property(x => x.AudienceScope).HasMaxLength(32).IsRequired();
            b.Property(x => x.RoleTargeting).HasMaxLength(32).IsRequired();
            b.HasIndex(x => x.CohortId);
        });
        modelBuilder.Entity<AnnouncementRead>(b =>
            b.HasIndex(x => new { x.AnnouncementId, x.UserId }).IsUnique());
        modelBuilder.Entity<Message>(b =>
        {
            b.Property(x => x.ThreadKey).HasMaxLength(80).IsRequired();
            b.Property(x => x.Body).HasMaxLength(4000).IsRequired();
            b.HasIndex(x => x.ThreadKey);
            b.HasIndex(x => x.RecipientId);
        });
        modelBuilder.Entity<Notification>(b =>
        {
            b.Property(x => x.EventKey).HasMaxLength(64).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Body).HasMaxLength(2000).IsRequired();
            b.Property(x => x.LinkPath).HasMaxLength(512);
            b.HasIndex(x => new { x.UserId, x.ReadAt });
        });

        // Platform services (M4)
        modelBuilder.Entity<NotificationPreference>(b =>
        {
            b.Property(x => x.EventKey).HasMaxLength(64).IsRequired();
            b.HasIndex(x => new { x.UserId, x.EventKey }).IsUnique();
        });
        modelBuilder.Entity<DisplayPreference>(b =>
        {
            b.Property(x => x.ReadingFont).HasMaxLength(64);
            b.HasIndex(x => x.UserId).IsUnique();
        });
        modelBuilder.Entity<SupportRequest>(b =>
        {
            b.Property(x => x.Category).HasMaxLength(64).IsRequired();
            b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            b.Property(x => x.ReplyToEmail).HasMaxLength(256).IsRequired();
            b.Property(x => x.ScreenshotUrl).HasMaxLength(512);
            b.HasIndex(x => x.CreatedAt);
        });

        // Guardian portal + public site (M5)
        modelBuilder.Entity<GuardianProfile>(b =>
        {
            b.Property(x => x.Relationship).HasMaxLength(64);
            b.Property(x => x.Phone).HasMaxLength(32);
            b.Property(x => x.EmergencyContactName).HasMaxLength(128);
            b.Property(x => x.EmergencyContactPhone).HasMaxLength(32);
            b.HasIndex(x => x.GuardianUserId).IsUnique();
        });
        modelBuilder.Entity<ContactRequest>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.Topic).HasMaxLength(128).IsRequired();
            b.Property(x => x.Message).HasMaxLength(4000).IsRequired();
            b.HasIndex(x => x.CreatedAt);
        });
        modelBuilder.Entity<VolunteerSignup>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(32);
            b.Property(x => x.AreasOfInterest).HasMaxLength(512);
            b.HasIndex(x => x.CreatedAt);
        });
        modelBuilder.Entity<DonationPledge>(b =>
        {
            b.Property(x => x.DonorName).HasMaxLength(128).IsRequired();
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.Tier).HasMaxLength(64).IsRequired();
            b.Property(x => x.MonthlyAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Frequency).HasMaxLength(32).IsRequired();
            b.Property(x => x.ProviderReference).HasMaxLength(256);
            b.HasIndex(x => x.CreatedAt);
        });
    }
}

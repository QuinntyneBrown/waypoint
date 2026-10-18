using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly ISensitiveDataProtector _sensitiveData;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ISensitiveDataProtector sensitiveData) : base(options)
    {
        _sensitiveData = sensitiveData;
    }

    internal string SensitiveDataKeyIdentifier => _sensitiveData.KeyIdentifier;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, SensitiveDataModelCacheKeyFactory>();
        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<YouthApplication> Applications => Set<YouthApplication>();
    public DbSet<GuardianConsent> GuardianConsents => Set<GuardianConsent>();
    public DbSet<GuardianConsentRequest> GuardianConsentRequests => Set<GuardianConsentRequest>();
    public DbSet<Cohort> Cohorts => Set<Cohort>();
    public DbSet<CohortMentor> CohortMentors => Set<CohortMentor>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<CodingLessonStep> CodingLessonSteps => Set<CodingLessonStep>();
    public DbSet<LessonMedia> LessonMedia => Set<LessonMedia>();
    public DbSet<VideoUploadSession> VideoUploadSessions => Set<VideoUploadSession>();
    public DbSet<VideoTranscodeJob> VideoTranscodeJobs => Set<VideoTranscodeJob>();
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
    public DbSet<CodingLessonWorkspace> CodingLessonWorkspaces => Set<CodingLessonWorkspace>();
    public DbSet<YouthTrackEnrollment> YouthTrackEnrollments => Set<YouthTrackEnrollment>();
    public DbSet<LessonReadingState> LessonReadingStates => Set<LessonReadingState>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<StoredSubmissionFile> StoredSubmissionFiles => Set<StoredSubmissionFile>();
    public DbSet<SubmissionTimelineEvent> SubmissionTimelineEvents => Set<SubmissionTimelineEvent>();
    public DbSet<ReflectionEntry> Reflections => Set<ReflectionEntry>();
    public DbSet<CompetencyAssessment> CompetencyAssessments => Set<CompetencyAssessment>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<Flag> Flags => Set<Flag>();
    public DbSet<AssignedWork> AssignedWork => Set<AssignedWork>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceWriteReceipt> AttendanceWriteReceipts => Set<AttendanceWriteReceipt>();
    public DbSet<SessionRsvp> SessionRsvps => Set<SessionRsvp>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementRead> AnnouncementReads => Set<AnnouncementRead>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ScheduledNotification> ScheduledNotifications => Set<ScheduledNotification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<DisplayPreference> DisplayPreferences => Set<DisplayPreference>();
    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();
    public DbSet<StoredUserFile> StoredUserFiles => Set<StoredUserFile>();
    public DbSet<DataSubjectRequest> DataSubjectRequests => Set<DataSubjectRequest>();
    public DbSet<OutboundEmail> OutboundEmails => Set<OutboundEmail>();
    public DbSet<GuardianProfile> GuardianProfiles => Set<GuardianProfile>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<VolunteerSignup> VolunteerSignups => Set<VolunteerSignup>();
    public DbSet<DonationPledge> DonationPledges => Set<DonationPledge>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureAuditEntriesAreAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureAuditEntriesAreAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureAuditEntriesAreAppendOnly()
    {
        if (ChangeTracker.Entries<AuditEntry>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
        {
            throw new InvalidOperationException("Audit entries are append-only and cannot be changed or deleted.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var protectedText = new ValueConverter<string, string>(
            value => _sensitiveData.Protect(value),
            value => _sensitiveData.Unprotect(value));
        var protectedNullableText = new ValueConverter<string?, string?>(
            value => value == null ? null : _sensitiveData.Protect(value),
            value => value == null ? null : _sensitiveData.Unprotect(value));
        var protectedBytes = new ValueConverter<byte[], byte[]>(
            value => _sensitiveData.Protect(value),
            value => _sensitiveData.Unprotect(value));
        var protectedBytesComparer = new ValueComparer<byte[]>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
            value => value.ToArray());

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

        modelBuilder.Entity<DataSubjectRequest>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Reference).IsUnique();
            b.HasIndex(x => new { x.Status, x.RequestedAt });
            b.HasIndex(x => x.SubjectUserId);
            b.Property(x => x.Reference).HasMaxLength(32).IsRequired();
            b.Property(x => x.ActionType).HasMaxLength(32).IsRequired();
            b.Property(x => x.Details).HasConversion(protectedNullableText).HasMaxLength(12000);
            b.Property(x => x.RequestedDisplayName).HasConversion(protectedNullableText).HasMaxLength(12000);
            b.Property(x => x.RequestedEmail).HasConversion(protectedNullableText).HasMaxLength(12000);
            b.Property(x => x.RequestedPronouns).HasConversion(protectedNullableText).HasMaxLength(12000);
            b.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
            b.Property(x => x.ResolutionNote).HasConversion(protectedNullableText).HasMaxLength(12000);
        });

        modelBuilder.Entity<OutboundEmail>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.ToEmail).HasConversion(protectedText).HasMaxLength(12000).IsRequired();
            b.Property(x => x.Subject).HasConversion(protectedText).HasMaxLength(12000).IsRequired();
            b.Property(x => x.Body).HasConversion(protectedText).HasMaxLength(40000).IsRequired();
            b.Property(x => x.IdempotencyKey).HasMaxLength(200);
            b.Property(x => x.LastErrorCode).HasMaxLength(128);
            b.HasIndex(x => x.IdempotencyKey).IsUnique();
            b.HasIndex(x => x.RecipientUserId);
            b.HasIndex(x => x.SubjectUserId);
            b.HasIndex(x => new { x.SentAt, x.AvailableAt, x.LeaseUntil });
            b.HasOne<User>().WithMany().HasForeignKey(x => x.RecipientUserId)
                .OnDelete(DeleteBehavior.NoAction);
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
            b.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
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
            b.HasIndex(x => x.SequenceNumber)
                .IsUnique()
                .HasFilter("[SequenceNumber] IS NOT NULL");
            b.Property(x => x.Action).HasMaxLength(128).IsRequired();
            b.Property(x => x.TargetType).HasMaxLength(128);
            b.Property(x => x.Source).HasConversion(protectedNullableText).HasMaxLength(2048);
            b.Property(x => x.Details).HasConversion(protectedNullableText).HasMaxLength(12000);
            b.Property(x => x.PreviousHash).HasMaxLength(64);
            b.Property(x => x.IntegrityHash).HasMaxLength(64);
        });

        modelBuilder.Entity<YouthApplication>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.ApplicantUserId);
            b.HasIndex(x => x.GuardianUserId);
            b.HasIndex(x => x.CreatedYouthUserId);
            b.Property(x => x.FirstName).HasConversion(protectedText).HasMaxLength(1024).IsRequired();
            b.Property(x => x.LastName).HasConversion(protectedText).HasMaxLength(1024).IsRequired();
            b.Property(x => x.WhoIsFilling).HasConversion(protectedText).HasMaxLength(512).IsRequired();
            b.Property(x => x.Neighbourhood).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.CohortInterest).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.WhyInterested).HasConversion(protectedText).HasMaxLength(12000).IsRequired();
            b.Property(x => x.TrackInterest).HasConversion(protectedNullableText).HasMaxLength(2048);
            b.Property(x => x.GuardianName).HasConversion(protectedNullableText).HasMaxLength(2048);
            b.Property(x => x.GuardianEmail).HasConversion(protectedNullableText).HasMaxLength(2048);
            b.Property(x => x.GuardianPhone).HasConversion(protectedNullableText).HasMaxLength(1024);
            b.Property(x => x.DecisionNote).HasConversion(protectedNullableText).HasMaxLength(12000);
            b.Property(x => x.Status).HasConversion<int>();
            b.HasOne<User>().WithMany().HasForeignKey(x => x.ApplicantUserId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne<User>().WithMany().HasForeignKey(x => x.GuardianUserId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedYouthUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<GuardianConsent>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.YouthUserId);
            b.Property(x => x.GuardianName).HasConversion(protectedText).HasMaxLength(512).IsRequired();
            b.Property(x => x.Relationship).HasConversion(protectedText).HasMaxLength(256).IsRequired();
            b.Property(x => x.Signature).HasConversion(protectedText).HasMaxLength(512).IsRequired();
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
        modelBuilder.Entity<CodingLessonStep>(b =>
        {
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Instructions).HasMaxLength(4000).IsRequired();
            b.Property(x => x.CodeSample).HasMaxLength(50000).IsRequired();
            b.Property(x => x.ChecklistLabel).HasMaxLength(300).IsRequired();
            b.HasIndex(x => new { x.LessonId, x.Order }).IsUnique();
        });
        modelBuilder.Entity<LessonMedia>(b =>
        {
            b.HasIndex(x => x.LessonId);
            b.Property(x => x.MediaType).HasMaxLength(16).IsRequired();
            b.Property(x => x.Url).HasMaxLength(2048).IsRequired();
            b.Property(x => x.OriginalFileName).HasMaxLength(255);
            b.Property(x => x.ContentType).HasMaxLength(128);
            b.Property(x => x.FailureCode).HasMaxLength(64);
        });
        modelBuilder.Entity<VideoTranscodeJob>(b =>
        {
            b.Property(x => x.LastError).HasMaxLength(2000);
            b.HasIndex(x => x.MediaId).IsUnique();
            b.HasIndex(x => x.LeaseUntil);
        });
        modelBuilder.Entity<VideoUploadSession>(b =>
        {
            b.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            b.Property(x => x.ReceivedChunks).IsRequired();
            b.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
            b.HasIndex(x => x.LessonId);
            b.HasIndex(x => x.ExpiresAt);
        });
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
        modelBuilder.Entity<BadgeAward>(b =>
            b.HasIndex(x => new { x.BadgeId, x.YouthUserId }).IsUnique());
        modelBuilder.Entity<ProgressionStage>(b => b.Property(x => x.Name).HasMaxLength(128).IsRequired());
        modelBuilder.Entity<Competency>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.HasIndex(x => x.StageId);
        });

        // Program administration (M2)
        modelBuilder.Entity<OrgSettings>(b => b.Property(x => x.Name).HasMaxLength(128).IsRequired());
        modelBuilder.Entity<Term>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(128).IsRequired();
            b.Property(x => x.Status).HasMaxLength(16).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
        });
        modelBuilder.Entity<CohortMentor>(b =>
            b.HasIndex(x => new { x.CohortId, x.MentorId }).IsUnique());

        // Learning & mentoring (M3)
        modelBuilder.Entity<LessonProgress>(b => b.HasIndex(x => new { x.YouthUserId, x.LessonId }).IsUnique());
        modelBuilder.Entity<CodingLessonWorkspace>(b =>
        {
            b.Property(x => x.Code).HasConversion(protectedText);
            b.Property(x => x.CheckedStepIdsJson).HasMaxLength(4000).IsRequired();
            b.HasIndex(x => new { x.YouthUserId, x.LessonId }).IsUnique();
        });
        modelBuilder.Entity<YouthTrackEnrollment>(b =>
            b.HasIndex(x => new { x.YouthUserId, x.TrackId }).IsUnique());
        modelBuilder.Entity<LessonReadingState>(b => b.HasIndex(x => new { x.YouthUserId, x.LessonId }).IsUnique());
        modelBuilder.Entity<QuizAttempt>(b =>
        {
            b.HasIndex(x => new { x.YouthUserId, x.QuizId });
            b.Property(x => x.AnswersJson).HasConversion(protectedNullableText);
        });
        modelBuilder.Entity<Submission>(b =>
        {
            b.HasIndex(x => new { x.YouthUserId, x.AssignmentId });
            b.Property(x => x.BodyText).HasConversion(protectedNullableText);
            b.Property(x => x.YouthNote).HasConversion(protectedNullableText).HasMaxLength(4000);
            b.Property(x => x.MentorComment).HasConversion(protectedNullableText).HasMaxLength(12000);
            b.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        });
        modelBuilder.Entity<StoredSubmissionFile>(b =>
        {
            b.Property(x => x.OriginalFileName).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.ContentType).HasMaxLength(64).IsRequired();
            b.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
            var content = b.Property(x => x.Content)
                .HasConversion(protectedBytes)
                .IsRequired();
            content.Metadata.SetValueComparer(protectedBytesComparer);
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
            b.HasIndex(x => new { x.OwnerUserId, x.AssignmentId });
        });
        modelBuilder.Entity<SubmissionTimelineEvent>(b =>
        {
            b.Property(x => x.Type).HasMaxLength(32).IsRequired();
            b.HasIndex(x => new { x.SubmissionId, x.OccurredAt });
        });
        modelBuilder.Entity<ReflectionEntry>(b =>
        {
            b.HasIndex(x => new { x.YouthUserId, x.LessonId });
            b.Property(x => x.Text).HasConversion(protectedNullableText).HasMaxLength(24000);
            b.Property(x => x.Mood).HasConversion(protectedNullableText).HasMaxLength(256);
            b.Property(x => x.SecondaryJson).HasConversion(protectedNullableText).HasMaxLength(12000);
        });
        modelBuilder.Entity<CompetencyAssessment>(b =>
        {
            b.HasIndex(x => new { x.YouthUserId, x.CompetencyId });
            b.Property(x => x.Notes).HasConversion(protectedNullableText);
        });
        modelBuilder.Entity<CheckIn>(b =>
        {
            b.HasIndex(x => x.YouthUserId);
            b.Property(x => x.Type).HasMaxLength(32).IsRequired();
            b.Property(x => x.GoalsDiscussed).HasConversion(protectedNullableText);
            b.Property(x => x.PrayerRequests).HasConversion(protectedNullableText);
            b.Property(x => x.ActionItems).HasConversion(protectedNullableText);
        });
        modelBuilder.Entity<Flag>(b =>
        {
            b.HasIndex(x => x.YouthUserId)
                .HasFilter("[ResolvedAt] IS NULL")
                .IsUnique();
            b.Property(x => x.Reason).HasConversion(protectedText).HasMaxLength(12000).IsRequired();
            b.Property(x => x.AutomaticReason).HasConversion(protectedNullableText).HasMaxLength(12000);
            b.Property(x => x.AutomaticSignalKeys).HasMaxLength(128);
            b.Property(x => x.ResolutionReason).HasConversion(protectedNullableText).HasMaxLength(1024);
        });
        modelBuilder.Entity<AssignedWork>(b =>
        {
            b.HasIndex(x => x.YouthUserId);
            b.Property(x => x.ItemType).HasMaxLength(32).IsRequired();
            b.Property(x => x.ReminderCadence).HasMaxLength(32);
            b.Property(x => x.Message).HasConversion(protectedNullableText).HasMaxLength(24000);
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
            b.Property(x => x.Note).HasConversion(protectedNullableText).HasMaxLength(4000);
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<GuardianConsentRequest>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.HasIndex(x => new { x.YouthUserId, x.UsedAt });
            b.Property(x => x.Channel).HasMaxLength(16).IsRequired();
            b.Property(x => x.Destination).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
        });
        modelBuilder.Entity<AttendanceWriteReceipt>(b =>
        {
            b.HasIndex(x => new { x.OwnerUserId, x.IdempotencyKey }).IsUnique();
            b.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
            b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        });
        modelBuilder.Entity<SessionRsvp>(b =>
        {
            b.HasIndex(x => new { x.SessionId, x.YouthUserId }).IsUnique();
            b.Property(x => x.Response).HasConversion<int>();
            b.Property(x => x.Note).HasConversion(protectedNullableText).HasMaxLength(8000);
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
            b.Property(x => x.Body).HasConversion(protectedText).HasMaxLength(12000).IsRequired();
            b.HasIndex(x => x.ThreadKey);
            b.HasIndex(x => x.RecipientId);
        });
        modelBuilder.Entity<Notification>(b =>
        {
            b.Property(x => x.EventKey).HasMaxLength(64).IsRequired();
            b.Property(x => x.Title).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.Body).HasConversion(protectedText).HasMaxLength(12000).IsRequired();
            b.Property(x => x.LinkPath).HasMaxLength(512);
            b.HasIndex(x => new { x.UserId, x.ReadAt });
            b.HasIndex(x => x.ScheduledNotificationId).IsUnique();
        });
        modelBuilder.Entity<ScheduledNotification>(b =>
        {
            b.Property(x => x.EventKey).HasMaxLength(64).IsRequired();
            b.Property(x => x.Title).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.Body).HasConversion(protectedText).HasMaxLength(12000).IsRequired();
            b.Property(x => x.LinkPath).HasMaxLength(512);
            b.Property(x => x.SourceType).HasMaxLength(64).IsRequired();
            b.Property(x => x.LastErrorCode).HasMaxLength(128);
            b.HasIndex(x => new { x.UserId, x.EventKey, x.SourceType, x.SourceId }).IsUnique();
            b.HasIndex(x => new { x.DeliveredAt, x.CancelledAt, x.DeliverAt });
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
            b.Property(x => x.Theme).HasMaxLength(16).IsRequired();
            b.HasIndex(x => x.UserId).IsUnique();
        });
        modelBuilder.Entity<SupportRequest>(b =>
        {
            b.Property(x => x.Category).HasMaxLength(64).IsRequired();
            b.Property(x => x.Description).HasConversion(protectedText).HasMaxLength(12000).IsRequired();
            b.Property(x => x.ReplyToEmail).HasConversion(protectedText).HasMaxLength(1024).IsRequired();
            b.HasIndex(x => x.CreatedAt);
        });
        modelBuilder.Entity<StoredUserFile>(b =>
        {
            b.Property(x => x.Purpose).HasConversion<int>();
            b.Property(x => x.OriginalFileName).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            b.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
            var content = b.Property(x => x.Content)
                .HasConversion(protectedBytes)
                .IsRequired();
            content.Metadata.SetValueComparer(protectedBytesComparer);
            b.HasIndex(x => new { x.OwnerUserId, x.Purpose });
        });

        // Guardian portal + public site (M5)
        modelBuilder.Entity<GuardianProfile>(b =>
        {
            b.Property(x => x.Relationship).HasConversion(protectedNullableText).HasMaxLength(256);
            b.Property(x => x.Phone).HasConversion(protectedNullableText).HasMaxLength(256);
            b.Property(x => x.EmergencyContactName).HasConversion(protectedNullableText).HasMaxLength(512);
            b.Property(x => x.EmergencyContactPhone).HasConversion(protectedNullableText).HasMaxLength(256);
            b.HasIndex(x => x.GuardianUserId).IsUnique();
        });
        modelBuilder.Entity<ContactRequest>(b =>
        {
            b.Property(x => x.Name).HasConversion(protectedText).HasMaxLength(1024).IsRequired();
            b.Property(x => x.Email).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.Topic).HasConversion(protectedText).HasMaxLength(1024).IsRequired();
            b.Property(x => x.Message).HasConversion(protectedText).HasMaxLength(24000).IsRequired();
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<User>().WithMany().HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<VolunteerSignup>(b =>
        {
            b.Property(x => x.Name).HasConversion(protectedText).HasMaxLength(1024).IsRequired();
            b.Property(x => x.Email).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.Phone).HasConversion(protectedNullableText).HasMaxLength(1024);
            b.Property(x => x.AreasOfInterest).HasConversion(protectedNullableText).HasMaxLength(4000);
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<User>().WithMany().HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<DonationPledge>(b =>
        {
            b.Property(x => x.DonorName).HasConversion(protectedText).HasMaxLength(1024).IsRequired();
            b.Property(x => x.Email).HasConversion(protectedText).HasMaxLength(2048).IsRequired();
            b.Property(x => x.Tier).HasMaxLength(64).IsRequired();
            b.Property(x => x.MonthlyAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Frequency).HasMaxLength(32).IsRequired();
            b.Property(x => x.ProviderReference).HasConversion(protectedNullableText).HasMaxLength(2048);
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<User>().WithMany().HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}

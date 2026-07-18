using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Domain;

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
        });
    }
}

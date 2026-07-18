using Microsoft.EntityFrameworkCore;
using WordUp.Domain;

namespace WordUp.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Invitation> Invitations { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    DbSet<YouthApplication> Applications { get; }
    DbSet<GuardianConsent> GuardianConsents { get; }
    DbSet<Cohort> Cohorts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

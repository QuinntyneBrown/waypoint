using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Authentication.Invitations;

internal static class InvitationAccountGuard
{
    private const string AccountExistsMessage = "An account with this email address already exists.";

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static async Task EnsureAvailableAsync(
        IAppDbContext db,
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        if (await AccountExistsAsync(db, normalizedEmail, cancellationToken))
        {
            throw new ConflictException(AccountExistsMessage);
        }
    }

    public static async Task EnsureAllAvailableAsync(
        IAppDbContext db,
        IReadOnlyCollection<string> normalizedEmails,
        CancellationToken cancellationToken)
    {
        if (normalizedEmails.Count == 0) return;

        if (await db.Users.AsNoTracking().AnyAsync(
                user => normalizedEmails.Contains(user.Email.ToLower()),
                cancellationToken))
        {
            throw new ConflictException(AccountExistsMessage);
        }
    }

    public static Task<bool> AccountExistsAsync(
        IAppDbContext db,
        string normalizedEmail,
        CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().AnyAsync(
            user => user.Email.ToLower() == normalizedEmail,
            cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ApplicationIntake;

internal static class ApplicationDecisionEmailRecipientResolver
{
    public static async Task<ApplicationDecisionEmailRecipient?> ResolveAsync(
        IAppDbContext db,
        YouthApplication application,
        CancellationToken cancellationToken)
    {
        // A supplied guardian address is the actual destination for a minor/referral. It may only
        // be account-linked when the guardian identity was authenticated or separately verified.
        if (!string.IsNullOrWhiteSpace(application.GuardianEmail))
        {
            return new ApplicationDecisionEmailRecipient(
                application.GuardianEmail,
                application.GuardianUserId);
        }

        // Adult self-applications do not collect guardian contact details. If the author was
        // authenticated (or later verified), deliver to that account so corrections readdress the
        // envelope and removal suppresses it. A guardian link is a final defensive fallback for
        // legacy records whose stored guardian address has already been cleared.
        var recipientUserId = application.ApplicantUserId ?? application.GuardianUserId;
        if (recipientUserId is not Guid linkedRecipientId)
        {
            return null;
        }

        var email = await db.Users
            .AsNoTracking()
            .Where(item => item.Id == linkedRecipientId)
            .Select(item => item.Email)
            .SingleOrDefaultAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(email)
            ? null
            : new ApplicationDecisionEmailRecipient(email, linkedRecipientId);
    }
}

internal sealed record ApplicationDecisionEmailRecipient(string Email, Guid? UserId);

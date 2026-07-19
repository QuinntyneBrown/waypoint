using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ConsentAndSafeguarding;

public sealed record ConsentPolicySnapshot(
    bool RequireReConsentEachTerm,
    bool RequireMediaConsent,
    Guid? CurrentTermId,
    string? CurrentTermName,
    DateTime? CurrentTermEnd,
    string OrganizationName,
    string? ContactEmail,
    string ConsentStatement)
{
    public bool IsParticipationValid(GuardianConsent consent, DateTime now)
    {
        if (!consent.IsParticipationValid(now))
        {
            return false;
        }
        if (!RequireReConsentEachTerm || CurrentTermId is null)
        {
            return true;
        }

        return consent.TermId == CurrentTermId
            || (consent.TermId is null
                && string.Equals(consent.Term, CurrentTermName, StringComparison.Ordinal));
    }

    public DateTime ConsentValidUntil(DateTime now) =>
        RequireReConsentEachTerm && CurrentTermEnd is { } termEnd
            ? termEnd
            : now.AddYears(1);
}

public static class ConsentPolicyEvaluator
{
    private const string DefaultStatement =
        "I give permission for this youth to participate in Word Up activities.";

    public static async Task<ConsentPolicySnapshot> LoadAsync(
        IAppDbContext db,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var settings = await db.OrgSettings
            .AsNoTracking()
            .Select(item => new
            {
                item.Name,
                item.DisplayName,
                item.ContactEmail,
                item.ConsentStatement,
                item.RequireReConsentEachTerm,
                item.RequireMediaConsent,
            })
            .FirstOrDefaultAsync(cancellationToken);
        var term = await db.Terms
            .AsNoTracking()
            .Where(item => item.Status == "Active"
                && item.StartDate <= now
                && item.EndDate >= now)
            .OrderByDescending(item => item.StartDate)
            .Select(item => new { item.Id, item.Name, item.EndDate })
            .FirstOrDefaultAsync(cancellationToken);

        return new ConsentPolicySnapshot(
            settings?.RequireReConsentEachTerm ?? true,
            settings?.RequireMediaConsent ?? false,
            term?.Id,
            term?.Name,
            term?.EndDate,
            settings?.DisplayName ?? settings?.Name ?? "Word Up",
            settings?.ContactEmail,
            string.IsNullOrWhiteSpace(settings?.ConsentStatement)
                ? DefaultStatement
                : settings.ConsentStatement);
    }

    public static async Task<bool> IsYouthParticipationAllowedAsync(
        IAppDbContext db,
        Guid youthUserId,
        Guid? guardianUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // Youth without a linked guardian are treated as adults and are not consent-gated.
        if (guardianUserId is null)
        {
            return true;
        }

        var policy = await LoadAsync(db, now, cancellationToken);
        var consents = await db.GuardianConsents
            .AsNoTracking()
            .Where(consent => consent.YouthUserId == youthUserId)
            .ToListAsync(cancellationToken);

        return consents.Any(consent => policy.IsParticipationValid(consent, now));
    }
}

using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Infrastructure;

public sealed class VerifiedRecordOwnershipClaimer(AppDbContext db)
    : IVerifiedRecordOwnershipClaimer
{
    public async Task<int> ClaimAsync(
        Guid subjectUserId,
        IReadOnlyList<Guid> contactRequestIds,
        IReadOnlyList<Guid> volunteerSignupIds,
        IReadOnlyList<Guid> donationPledgeIds,
        IReadOnlyList<Guid> applicantApplicationIds,
        IReadOnlyList<Guid> guardianApplicationIds,
        CancellationToken cancellationToken = default)
    {
        if (!db.Database.IsRelational())
        {
            return await ClaimTrackedAsync(
                subjectUserId,
                contactRequestIds,
                volunteerSignupIds,
                donationPledgeIds,
                applicantApplicationIds,
                guardianApplicationIds,
                cancellationToken);
        }
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Verified record ownership claims require an atomic database transaction.");
        }

        var linked = 0;
        if (contactRequestIds.Count > 0)
        {
            var affected = await db.ContactRequests
                .Where(item => contactRequestIds.Contains(item.Id)
                    && (item.OwnerUserId == null || item.OwnerUserId == subjectUserId))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.OwnerUserId, subjectUserId),
                    cancellationToken);
            EnsureClaimed(affected, contactRequestIds.Count);
            linked += affected;
        }
        if (volunteerSignupIds.Count > 0)
        {
            var affected = await db.VolunteerSignups
                .Where(item => volunteerSignupIds.Contains(item.Id)
                    && (item.OwnerUserId == null || item.OwnerUserId == subjectUserId))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.OwnerUserId, subjectUserId),
                    cancellationToken);
            EnsureClaimed(affected, volunteerSignupIds.Count);
            linked += affected;
        }
        if (donationPledgeIds.Count > 0)
        {
            var affected = await db.DonationPledges
                .Where(item => donationPledgeIds.Contains(item.Id)
                    && (item.OwnerUserId == null || item.OwnerUserId == subjectUserId))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.OwnerUserId, subjectUserId),
                    cancellationToken);
            EnsureClaimed(affected, donationPledgeIds.Count);
            linked += affected;
        }
        if (applicantApplicationIds.Count > 0)
        {
            var affected = await db.Applications
                .Where(item => applicantApplicationIds.Contains(item.Id)
                    && (item.ApplicantUserId == null || item.ApplicantUserId == subjectUserId))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.ApplicantUserId, subjectUserId),
                    cancellationToken);
            EnsureClaimed(affected, applicantApplicationIds.Count);
            linked += affected;
        }
        if (guardianApplicationIds.Count > 0)
        {
            var affected = await db.Applications
                .Where(item => guardianApplicationIds.Contains(item.Id)
                    && (item.GuardianUserId == null || item.GuardianUserId == subjectUserId))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.GuardianUserId, subjectUserId),
                    cancellationToken);
            EnsureClaimed(affected, guardianApplicationIds.Count);
            linked += affected;
        }

        return linked;
    }

    private async Task<int> ClaimTrackedAsync(
        Guid subjectUserId,
        IReadOnlyList<Guid> contactRequestIds,
        IReadOnlyList<Guid> volunteerSignupIds,
        IReadOnlyList<Guid> donationPledgeIds,
        IReadOnlyList<Guid> applicantApplicationIds,
        IReadOnlyList<Guid> guardianApplicationIds,
        CancellationToken cancellationToken)
    {
        var linked = 0;
        if (contactRequestIds.Count > 0)
        {
            var rows = await db.ContactRequests
                .Where(item => contactRequestIds.Contains(item.Id))
                .ToListAsync(cancellationToken);
            EnsureAttachable(rows.Select(item => item.OwnerUserId), rows.Count, contactRequestIds.Count, subjectUserId);
            foreach (var row in rows) row.OwnerUserId = subjectUserId;
            linked += rows.Count;
        }
        if (volunteerSignupIds.Count > 0)
        {
            var rows = await db.VolunteerSignups
                .Where(item => volunteerSignupIds.Contains(item.Id))
                .ToListAsync(cancellationToken);
            EnsureAttachable(rows.Select(item => item.OwnerUserId), rows.Count, volunteerSignupIds.Count, subjectUserId);
            foreach (var row in rows) row.OwnerUserId = subjectUserId;
            linked += rows.Count;
        }
        if (donationPledgeIds.Count > 0)
        {
            var rows = await db.DonationPledges
                .Where(item => donationPledgeIds.Contains(item.Id))
                .ToListAsync(cancellationToken);
            EnsureAttachable(rows.Select(item => item.OwnerUserId), rows.Count, donationPledgeIds.Count, subjectUserId);
            foreach (var row in rows) row.OwnerUserId = subjectUserId;
            linked += rows.Count;
        }
        if (applicantApplicationIds.Count > 0)
        {
            var rows = await db.Applications
                .Where(item => applicantApplicationIds.Contains(item.Id))
                .ToListAsync(cancellationToken);
            EnsureAttachable(rows.Select(item => item.ApplicantUserId), rows.Count, applicantApplicationIds.Count, subjectUserId);
            foreach (var row in rows) row.ApplicantUserId = subjectUserId;
            linked += rows.Count;
        }
        if (guardianApplicationIds.Count > 0)
        {
            var rows = await db.Applications
                .Where(item => guardianApplicationIds.Contains(item.Id))
                .ToListAsync(cancellationToken);
            EnsureAttachable(rows.Select(item => item.GuardianUserId), rows.Count, guardianApplicationIds.Count, subjectUserId);
            foreach (var row in rows) row.GuardianUserId = subjectUserId;
            linked += rows.Count;
        }

        return linked;
    }

    private static void EnsureAttachable(
        IEnumerable<Guid?> owners,
        int foundCount,
        int requestedCount,
        Guid subjectUserId)
    {
        if (foundCount != requestedCount
            || owners.Any(ownerId => ownerId is Guid value && value != subjectUserId))
        {
            throw Conflict();
        }
    }

    private static void EnsureClaimed(int affected, int requested)
    {
        if (affected != requested)
        {
            throw Conflict();
        }
    }

    private static ConflictException Conflict() =>
        new("A verified record is unavailable or already linked to another account.");
}

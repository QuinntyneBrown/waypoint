using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.GuardianPortal;

/// <summary>
/// Single source of the guardian&lt;-&gt;child ownership check (L2-GUARDIAN-001 AC3 / L2-AUTH-010).
/// A youth belongs to a guardian when the youth's <see cref="User.GuardianUserId"/> equals the
/// signed-in guardian's own user id. Every guardian-portal handler that touches a child's data
/// resolves the child through here so the rule lives in exactly one place.
/// </summary>
internal static class GuardianChildLink
{
    /// <summary>Loads the child and throws <see cref="ForbiddenException"/> unless it is linked to the
    /// current guardian; throws <see cref="NotFoundException"/> when the child does not exist.</summary>
    public static async Task<User> ResolveChildAsync(
        IAppDbContext db, ICurrentUser currentUser, Guid childId, CancellationToken ct)
    {
        var child = await db.Users.FirstOrDefaultAsync(u => u.Id == childId, ct)
            ?? throw new NotFoundException("Child not found.");

        if (child.GuardianUserId is null || child.GuardianUserId != currentUser.UserId)
        {
            throw new ForbiddenException("You may only view your own child's information.");
        }

        return child;
    }

    /// <summary>The guardian's linked children (youth whose GuardianUserId is the current guardian).</summary>
    public static async Task<IReadOnlyList<User>> ResolveChildrenAsync(
        IAppDbContext db, ICurrentUser currentUser, CancellationToken ct)
    {
        var guardianId = currentUser.UserId;
        return await db.Users
            .Where(u => u.GuardianUserId == guardianId)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(ct);
    }

    /// <summary>The guardian's single/first linked child id, or null when none is linked.</summary>
    public static async Task<Guid?> ResolveChildIdAsync(
        IAppDbContext db, ICurrentUser currentUser, CancellationToken ct)
    {
        var guardianId = currentUser.UserId;
        var ids = await db.Users
            .Where(u => u.GuardianUserId == guardianId)
            .OrderBy(u => u.CreatedAt)
            .Select(u => u.Id)
            .ToListAsync(ct);
        return ids.Count == 0 ? null : ids[0];
    }
}

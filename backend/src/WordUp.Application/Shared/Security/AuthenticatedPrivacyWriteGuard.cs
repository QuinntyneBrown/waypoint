using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Shared.Security;

/// <summary>
/// Serializes account-linked PII writes with correction/removal and verifies that the authenticated
/// principal still represents an active account after the transaction-owned privacy lock is held.
/// Anonymous callers remain anonymous and do not acquire an account lock.
/// </summary>
public sealed class AuthenticatedPrivacyWriteGuard
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IUserPrivacyLock _privacyLock;

    public AuthenticatedPrivacyWriteGuard(
        IAppDbContext db,
        ICurrentUser currentUser,
        IUserPrivacyLock privacyLock)
    {
        _db = db;
        _currentUser = currentUser;
        _privacyLock = privacyLock;
    }

    public async Task<Guid?> AcquireAuthorAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return null;
        }

        var userId = _currentUser.UserId
            ?? throw AccountStateChanged();
        var tokenSessionVersion = _currentUser.SessionVersion;

        // SQL Server retains this application lock through the surrounding IAtomicRequest
        // transaction. A concurrent removal either wins before this check or waits until the
        // linked PII write commits and can then anonymize it.
        await _privacyLock.AcquireAsync(userId, cancellationToken);

        var account = await _db.Users
            .AsNoTracking()
            .Where(item => item.Id == userId)
            .Select(item => new
            {
                item.Status,
                item.SessionVersion,
                item.PersonalDataAnonymizedAt,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (account is null
            || account.Status != UserStatus.Active
            || account.PersonalDataAnonymizedAt is not null
            || (tokenSessionVersion.HasValue
                && account.SessionVersion != tokenSessionVersion.Value))
        {
            throw AccountStateChanged();
        }

        return userId;
    }

    private static ConflictException AccountStateChanged() =>
        new("The account state changed while the request was being processed. Please retry after signing in again.");
}

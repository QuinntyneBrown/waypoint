using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.Privacy;

public record RunRetentionCommand : IRequest<RetentionRunResult>, IAtomicRequest;
public record RetentionRunResult(int AnonymizedCount, DateTime Cutoff);

public sealed class RunRetentionCommandHandler : IRequestHandler<RunRetentionCommand, RetentionRunResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IDataRetentionPolicy _policy;
    private readonly PersonalDataAnonymizer _anonymizer;
    private readonly IAuditLogger _audit;
    private readonly IUserPrivacyLock _privacyLock;

    public RunRetentionCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IDataRetentionPolicy policy,
        PersonalDataAnonymizer anonymizer,
        IAuditLogger audit,
        IUserPrivacyLock privacyLock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _policy = policy;
        _anonymizer = anonymizer;
        _audit = audit;
        _privacyLock = privacyLock;
    }

    public async Task<RetentionRunResult> Handle(
        RunRetentionCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUser.UserId ?? throw new ForbiddenException();
        if (_currentUser.Role != Roles.ProgramAdministrator) throw new ForbiddenException();

        var now = _clock.UtcNow;
        var cutoff = now.AddDays(-_policy.DeactivatedUserRetentionDays);
        var expiredIds = await _db.Users.AsNoTracking()
            .Where(item => (item.Status == UserStatus.Deactivated || item.Status == UserStatus.Removed)
                && item.PersonalDataAnonymizedAt == null
                && (item.DeactivatedAt ?? item.LastSignInAt ?? item.CreatedAt) <= cutoff)
            .OrderBy(item => item.Id)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var anonymizedCount = 0;
        foreach (var userId in expiredIds)
        {
            await _privacyLock.AcquireAsync(userId, cancellationToken);
            var user = await _db.Users.FirstOrDefaultAsync(
                item => item.Id == userId
                    && (item.Status == UserStatus.Deactivated || item.Status == UserStatus.Removed)
                    && item.PersonalDataAnonymizedAt == null
                    && (item.DeactivatedAt ?? item.LastSignInAt ?? item.CreatedAt) <= cutoff,
                cancellationToken);
            if (user is null) continue;
            await _anonymizer.AnonymizeAsync(user, now, cancellationToken);
            anonymizedCount++;
        }

        await _audit.LogAsync(
            "RetentionAnonymizationRun",
            actorId,
            "User",
            details: $"cutoff={cutoff:O}; anonymized={anonymizedCount}",
            cancellationToken: cancellationToken);
        return new RetentionRunResult(anonymizedCount, cutoff);
    }
}

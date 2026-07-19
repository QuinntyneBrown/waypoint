using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.Privacy;

public record RunRetentionCommand : IRequest<RetentionRunResult>;
public record RetentionRunResult(int AnonymizedCount, DateTime Cutoff);

public sealed class RunRetentionCommandHandler : IRequestHandler<RunRetentionCommand, RetentionRunResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IDataRetentionPolicy _policy;
    private readonly PersonalDataAnonymizer _anonymizer;
    private readonly IAuditLogger _audit;

    public RunRetentionCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IDataRetentionPolicy policy,
        PersonalDataAnonymizer anonymizer,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _policy = policy;
        _anonymizer = anonymizer;
        _audit = audit;
    }

    public async Task<RetentionRunResult> Handle(
        RunRetentionCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUser.UserId ?? throw new ForbiddenException();
        if (_currentUser.Role != Roles.ProgramAdministrator) throw new ForbiddenException();

        var now = _clock.UtcNow;
        var cutoff = now.AddDays(-_policy.DeactivatedUserRetentionDays);
        var expired = await _db.Users
            .Where(item => (item.Status == UserStatus.Deactivated || item.Status == UserStatus.Removed)
                && item.PersonalDataAnonymizedAt == null
                && (item.DeactivatedAt ?? item.LastSignInAt ?? item.CreatedAt) <= cutoff)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        foreach (var user in expired)
        {
            await _anonymizer.AnonymizeAsync(user, now, cancellationToken);
        }

        await _audit.LogAsync(
            "RetentionAnonymizationRun",
            actorId,
            "User",
            details: $"cutoff={cutoff:O}; anonymized={expired.Count}",
            cancellationToken: cancellationToken);
        return new RetentionRunResult(expired.Count, cutoff);
    }
}

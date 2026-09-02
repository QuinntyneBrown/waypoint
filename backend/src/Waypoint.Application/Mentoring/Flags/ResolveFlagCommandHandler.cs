using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Mentoring.Flags;

public class ResolveFlagCommandHandler : IRequestHandler<ResolveFlagCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly AtRiskEvaluator _evaluator;

    public ResolveFlagCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        AtRiskEvaluator evaluator)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _evaluator = evaluator;
    }

    public async Task Handle(ResolveFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _db.Flags.FirstOrDefaultAsync(f => f.Id == request.FlagId, cancellationToken)
            ?? throw new NotFoundException("Flag not found.");

        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == flag.YouthUserId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // A mentor may only resolve flags on their assigned youth; a Program Administrator may resolve any (L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        if (flag.ResolvedAt is not null)
        {
            throw new ConflictException("Flag is already resolved.");
        }

        flag.ResolvedAt = _clock.UtcNow;
        flag.ResolutionReason = request.ResolutionReason;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "YouthUnflagged",
            _currentUser.UserId,
            "Flag",
            flag.Id,
            isPrivate: true,
            details: flag.IsAutomatic ? "category=automatic; outcome=resolved" : "category=manual; outcome=resolved",
            cancellationToken: cancellationToken);
        await _evaluator.EvaluateAsync(
            new[] { youth.Id },
            justResolvedFlagId: flag.Id,
            cancellationToken: cancellationToken);
    }
}

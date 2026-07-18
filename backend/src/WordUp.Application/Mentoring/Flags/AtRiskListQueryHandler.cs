using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.Flags;

public class AtRiskListQueryHandler : IRequestHandler<AtRiskListQuery, IReadOnlyList<AtRiskYouth>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly AtRiskEvaluator _evaluator;

    public AtRiskListQueryHandler(IAppDbContext db, ICurrentUser currentUser, AtRiskEvaluator evaluator)
    {
        _db = db;
        _currentUser = currentUser;
        _evaluator = evaluator;
    }

    public async Task<IReadOnlyList<AtRiskYouth>> Handle(AtRiskListQuery request, CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId ?? throw new ForbiddenException();

        var youthQuery = _db.Users.Where(user => user.Role == Roles.Youth);
        if (_currentUser.Role == Roles.Mentor)
        {
            youthQuery = youthQuery.Where(user => user.MentorId == mentorId);
        }
        var youth = await youthQuery
            .Select(user => new { user.Id, user.DisplayName })
            .ToListAsync(cancellationToken);
        var youthIds = youth.Select(user => user.Id).ToList();
        await _evaluator.EvaluateAsync(youthIds, cancellationToken: cancellationToken);

        var flags = await _db.Flags
            .Where(flag => youthIds.Contains(flag.YouthUserId) && flag.ResolvedAt == null)
            .OrderByDescending(flag => flag.FlaggedAt)
            .ToListAsync(cancellationToken);
        var actorIds = flags
            .Where(flag => !flag.IsAutomatic)
            .Select(flag => flag.FlaggedByUserId)
            .Distinct()
            .ToList();
        var actorNames = await _db.Users
            .Where(user => actorIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
        var youthNames = youth.ToDictionary(user => user.Id, user => user.DisplayName);

        return flags.Select(flag => new AtRiskYouth(
            flag.Id,
            flag.YouthUserId,
            youthNames.GetValueOrDefault(flag.YouthUserId, "Youth"),
            Reasons(flag),
            flag.FlaggedAt,
            flag.IsAutomatic
                ? "Automated evaluation"
                : actorNames.GetValueOrDefault(flag.FlaggedByUserId, "Program staff"),
            flag.IsAutomatic)).ToList();
    }

    private static IReadOnlyList<string> Reasons(Flag flag)
    {
        var reasons = new List<string> { flag.Reason };
        if (!flag.IsAutomatic
            && !string.IsNullOrWhiteSpace(flag.AutomaticReason)
            && !string.Equals(flag.Reason, flag.AutomaticReason, StringComparison.Ordinal))
        {
            reasons.Add(flag.AutomaticReason);
        }
        return reasons;
    }
}

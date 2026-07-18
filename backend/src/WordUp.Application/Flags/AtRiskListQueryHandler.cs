using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Flags;

public class AtRiskListQueryHandler : IRequestHandler<AtRiskListQuery, IReadOnlyList<AtRiskYouth>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AtRiskListQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AtRiskYouth>> Handle(AtRiskListQuery request, CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId ?? throw new ForbiddenException();

        // Only the mentor's assigned youth (L2-AUTH-010).
        var query =
            from f in _db.Flags
            join u in _db.Users on f.YouthUserId equals u.Id
            where f.ResolvedAt == null && u.MentorId == mentorId && u.Role == Roles.Youth
            orderby f.FlaggedAt descending
            select new AtRiskYouth(u.Id, u.DisplayName, f.Reason);

        return await query.ToListAsync(cancellationToken);
    }
}

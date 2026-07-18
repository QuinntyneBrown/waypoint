using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.MentorRoster;

public class YouthDetailQueryHandler : IRequestHandler<YouthDetailQuery, YouthDetail>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public YouthDetailQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<YouthDetail> Handle(YouthDetailQuery request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // A mentor may only view their assigned youth; a Program Administrator may view any (L2-MENTOR-002 AC3, L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        // Latest active flag for the at-risk banner (L2-MENTOR-002 AC2).
        var flag = await _db.Flags
            .Where(f => f.YouthUserId == youth.Id && f.ResolvedAt == null)
            .OrderByDescending(f => f.FlaggedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new YouthDetail(
            youth.Id,
            youth.DisplayName,
            youth.Email,
            youth.CohortId,
            youth.GuardianUserId,
            flag is not null,
            flag?.Reason,
            flag?.FlaggedByUserId,
            flag?.FlaggedAt);
    }
}

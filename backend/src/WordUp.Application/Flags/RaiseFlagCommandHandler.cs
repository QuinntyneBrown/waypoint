using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Flags;

public class RaiseFlagCommandHandler : IRequestHandler<RaiseFlagCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public RaiseFlagCommandHandler(IAppDbContext db, IClock clock, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(RaiseFlagCommand request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // A mentor may only flag their assigned youth; a Program Administrator may flag any (L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var flag = new Flag
        {
            YouthUserId = youth.Id,
            Reason = request.Reason,
            FlaggedByUserId = _currentUser.UserId ?? Guid.Empty,
            FlaggedAt = _clock.UtcNow,
        };

        _db.Flags.Add(flag);
        await _db.SaveChangesAsync(cancellationToken);

        // Visible to the director (L2-MENTOR-007).
        await _audit.LogAsync("YouthFlagged", _currentUser.UserId, "User", youth.Id, details: request.Reason);
    }
}

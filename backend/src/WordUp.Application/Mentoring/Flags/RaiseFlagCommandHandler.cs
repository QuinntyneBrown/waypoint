using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.Flags;

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

        if (youth.Role != Roles.Youth)
        {
            throw new NotFoundException("Youth not found.");
        }

        var active = await _db.Flags.FirstOrDefaultAsync(
            flag => flag.YouthUserId == youth.Id && flag.ResolvedAt == null,
            cancellationToken);
        if (active is not null)
        {
            if (string.Equals(active.Reason, request.Reason, StringComparison.OrdinalIgnoreCase)) return;
            throw new ConflictException("This youth already has an active at-risk flag.");
        }

        var flag = new Flag
        {
            YouthUserId = youth.Id,
            Reason = request.Reason,
            FlaggedByUserId = _currentUser.UserId ?? Guid.Empty,
            FlaggedAt = _clock.UtcNow,
            IsAutomatic = false,
        };

        _db.Flags.Add(flag);
        await _db.SaveChangesAsync(cancellationToken);

        // Visible to the director (L2-MENTOR-007).
        await _audit.LogAsync(
            "YouthFlagged",
            _currentUser.UserId,
            "Flag",
            flag.Id,
            isPrivate: true,
            details: "category=manual",
            cancellationToken: cancellationToken);
    }
}

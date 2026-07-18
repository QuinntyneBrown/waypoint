using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.Flags;

public class ResolveFlagCommandHandler : IRequestHandler<ResolveFlagCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ResolveFlagCommandHandler(IAppDbContext db, IClock clock, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
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

        await _audit.LogAsync("YouthUnflagged", _currentUser.UserId, "User", youth.Id, details: request.ResolutionReason);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.People;

public class ChangeRoleCommandHandler : IRequestHandler<ChangeRoleCommand>
{
    private readonly IAppDbContext _db;
    private readonly ISessionIssuer _sessions;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ChangeRoleCommandHandler(
        IAppDbContext db,
        ISessionIssuer sessions,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _sessions = sessions;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ChangeRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");
        if (user.Status == UserStatus.Removed)
        {
            throw new ConflictException("A removed person's role cannot be changed.");
        }
        if (user.Role == request.Role)
        {
            return;
        }

        var previousRole = user.Role;
        user.Role = request.Role;
        await _db.SaveChangesAsync(cancellationToken);
        await _sessions.RevokeAllAsync(user.Id, cancellationToken);

        await _audit.LogAsync(
            "RoleChanged",
            _currentUser.UserId,
            "User",
            user.Id,
            details: $"{previousRole} -> {request.Role}");
    }
}

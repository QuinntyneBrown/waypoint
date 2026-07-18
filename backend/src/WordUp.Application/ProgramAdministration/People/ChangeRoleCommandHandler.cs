using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.People;

public class ChangeRoleCommandHandler : IRequestHandler<ChangeRoleCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ChangeRoleCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ChangeRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");

        user.Role = request.Role;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("RoleChanged", _currentUser.UserId, "User", user.Id, details: request.Role);
    }
}

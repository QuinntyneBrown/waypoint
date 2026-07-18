using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.People;

public class ReactivatePersonCommandHandler : IRequestHandler<ReactivatePersonCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ReactivatePersonCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ReactivatePersonCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");

        user.Status = UserStatus.Active;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("AccountReactivated", _currentUser.UserId, "User", user.Id);
    }
}

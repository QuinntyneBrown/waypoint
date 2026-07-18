using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Invitations;

public class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public RevokeInvitationCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _db.Invitations.FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken)
            ?? throw new NotFoundException("Invitation not found.");

        invitation.Status = InvitationStatus.Revoked;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("InvitationRevoked", _currentUser.UserId, "Invitation", invitation.Id);
    }
}

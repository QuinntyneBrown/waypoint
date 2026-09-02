using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Authentication.Invitations;

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
        var invitation = await InvitationScope.LoadManagedAsync(
            _db,
            _currentUser,
            request.InvitationId,
            cancellationToken);

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ConflictException("Only pending invitations can be revoked.");
        }

        invitation.Status = InvitationStatus.Revoked;
        invitation.ConcurrencyStamp = Guid.NewGuid();
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("This invitation changed before it could be revoked.");
        }
        await _audit.LogAsync("InvitationRevoked", _currentUser.UserId, "Invitation", invitation.Id);
    }
}

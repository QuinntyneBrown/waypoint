using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Authentication.Invitations;

public class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationLinkBuilder _links;

    public ResendInvitationCommandHandler(
        IAppDbContext db,
        ISecureTokens tokens,
        IClock clock,
        IEmailSender email,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IApplicationLinkBuilder links)
    {
        _db = db;
        _tokens = tokens;
        _clock = clock;
        _email = email;
        _audit = audit;
        _currentUser = currentUser;
        _links = links;
    }

    public async Task Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await InvitationScope.LoadManagedAsync(
            _db,
            _currentUser,
            request.InvitationId,
            cancellationToken);

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvitationInvalidException("Only pending invitations can be resent.");
        }

        await InvitationAccountGuard.EnsureAvailableAsync(_db, invitation.Email, cancellationToken);

        var (raw, hash) = _tokens.Create();
        invitation.TokenHash = hash;
        invitation.ExpiresAt = _clock.UtcNow.AddDays(InvitationPolicy.ExpiryDays);
        invitation.ConcurrencyStamp = Guid.NewGuid();
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvitationInvalidException("This invitation changed before it could be resent.");
        }

        await InvitationEmail.SendAsync(_email, _links, invitation.Email, raw, cancellationToken);
        await _audit.LogAsync("InvitationResent", _currentUser.UserId, "Invitation", invitation.Id);
    }
}

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

    public ResendInvitationCommandHandler(
        IAppDbContext db,
        ISecureTokens tokens,
        IClock clock,
        IEmailSender email,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _tokens = tokens;
        _clock = clock;
        _email = email;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _db.Invitations.FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken)
            ?? throw new NotFoundException("Invitation not found.");

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvitationInvalidException("Only pending invitations can be resent.");
        }

        var (raw, hash) = _tokens.Create();
        invitation.TokenHash = hash;
        invitation.ExpiresAt = _clock.UtcNow.AddDays(InvitationPolicy.ExpiryDays);
        await _db.SaveChangesAsync(cancellationToken);

        await InvitationEmail.SendAsync(_email, invitation.Email, raw, cancellationToken);
        await _audit.LogAsync("InvitationResent", _currentUser.UserId, "Invitation", invitation.Id);
    }
}

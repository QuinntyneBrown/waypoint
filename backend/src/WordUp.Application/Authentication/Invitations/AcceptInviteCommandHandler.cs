using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;

namespace WordUp.Application.Authentication.Invitations;

public class AcceptInviteCommandHandler : IRequestHandler<AcceptInviteCommand, AuthResult>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;
    private readonly ISessionIssuer _session;
    private readonly IAuditLogger _audit;

    public AcceptInviteCommandHandler(
        IAppDbContext db,
        ISecureTokens tokens,
        IPasswordHasher hasher,
        IClock clock,
        ISessionIssuer session,
        IAuditLogger audit)
    {
        _db = db;
        _tokens = tokens;
        _hasher = hasher;
        _clock = clock;
        _session = session;
        _audit = audit;
    }

    public async Task<AuthResult> Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokens.Hash(request.Token);
        var now = _clock.UtcNow;

        var invitation = await _db.Invitations.FirstOrDefaultAsync(i => i.TokenHash == hash, cancellationToken);
        if (invitation is null || !invitation.IsAcceptable(now))
        {
            // Expired, revoked, already-used, or unknown (L2-AUTH-004 AC2/AC3/AC4).
            throw new InvitationInvalidException();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == invitation.Email, cancellationToken);
        if (user is null)
        {
            user = new User { Email = invitation.Email, CreatedAt = now };
            _db.Users.Add(user);
        }

        user.DisplayName = request.DisplayName.Trim();
        user.PasswordHash = _hasher.Hash(request.Password);
        user.Role = invitation.Role;
        user.CohortId = invitation.CohortId;
        user.MentorId = invitation.MentorId;
        user.Status = UserStatus.Active;
        user.OnboardingCompleted = false;

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("InvitationAccepted", user.Id, "Invitation", invitation.Id);

        return await _session.IssueAsync(user, cancellationToken);
    }
}

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
    private readonly IDatabaseErrorClassifier _databaseErrors;

    public AcceptInviteCommandHandler(
        IAppDbContext db,
        ISecureTokens tokens,
        IPasswordHasher hasher,
        IClock clock,
        ISessionIssuer session,
        IAuditLogger audit,
        IDatabaseErrorClassifier databaseErrors)
    {
        _db = db;
        _tokens = tokens;
        _hasher = hasher;
        _clock = clock;
        _session = session;
        _audit = audit;
        _databaseErrors = databaseErrors;
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

        // An account may have been provisioned after this invitation was issued. Never let an
        // invitation become a credential-reset or role-escalation path for that existing account.
        if (await InvitationAccountGuard.AccountExistsAsync(_db, invitation.Email, cancellationToken))
        {
            throw new InvitationInvalidException();
        }

        // Claim the single-use token before staging the account. A losing concurrent request then
        // cannot leave a partially inserted account even on providers without unique-index support.
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = now;
        invitation.ConcurrencyStamp = Guid.NewGuid();

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvitationInvalidException();
        }

        var user = new User
        {
            Email = invitation.Email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            Role = invitation.Role,
            CohortId = invitation.CohortId,
            MentorId = invitation.MentorId,
            Status = UserStatus.Active,
            OnboardingCompleted = false,
            CreatedAt = now,
        };
        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            _databaseErrors.IsUniqueConstraintViolation(exception, "IX_Users_Email"))
        {
            // An account created between the ownership check and this insert wins the unique-email
            // race. The invitation must never be allowed to mutate it or issue a session for it.
            throw new InvitationInvalidException();
        }
        await _audit.LogAsync("InvitationAccepted", user.Id, "Invitation", invitation.Id);

        return await _session.IssueAsync(user, cancellationToken);
    }
}

using MediatR;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Invitations;

public class InviteCommandHandler : IRequestHandler<InviteCommand, InviteResult>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public InviteCommandHandler(
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

    public async Task<InviteResult> Handle(InviteCommand request, CancellationToken cancellationToken)
    {
        // A mentor may only invite youth; a Program Administrator may invite any role (L2-AUTH-003 AC1/AC4).
        if (_currentUser.Role == Roles.Mentor && request.Role != Roles.Youth)
        {
            throw new ForbiddenException("Mentors may only invite youth.");
        }

        var (raw, hash) = _tokens.Create();
        var now = _clock.UtcNow;
        var invitation = new Invitation
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            Role = request.Role,
            CohortId = request.CohortId,
            MentorId = request.MentorId,
            TokenHash = hash,
            Status = InvitationStatus.Pending,
            CreatedAt = now,
            ExpiresAt = now.AddDays(InvitationPolicy.ExpiryDays),
            CreatedByUserId = _currentUser.UserId,
        };

        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync(cancellationToken);

        await InvitationEmail.SendAsync(_email, invitation.Email, raw, cancellationToken);
        await _audit.LogAsync("InvitationCreated", _currentUser.UserId, "Invitation", invitation.Id);

        return new InviteResult(invitation.Id);
    }
}

using MediatR;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Authentication.Invitations;

public class InviteCommandHandler : IRequestHandler<InviteCommand, InviteResult>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationLinkBuilder _links;

    public InviteCommandHandler(
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

    public async Task<InviteResult> Handle(InviteCommand request, CancellationToken cancellationToken)
    {
        var scope = await InvitationScope.ValidateAssignmentAsync(
            _db,
            _currentUser,
            request.Role,
            request.CohortId,
            request.MentorId,
            cancellationToken);

        var email = InvitationAccountGuard.NormalizeEmail(request.Email);
        await InvitationAccountGuard.EnsureAvailableAsync(_db, email, cancellationToken);

        var (raw, hash) = _tokens.Create();
        var now = _clock.UtcNow;
        var invitation = new Invitation
        {
            Email = email,
            Role = request.Role,
            CohortId = scope.CohortId,
            MentorId = scope.MentorId,
            TokenHash = hash,
            Status = InvitationStatus.Pending,
            CreatedAt = now,
            ExpiresAt = now.AddDays(InvitationPolicy.ExpiryDays),
            CreatedByUserId = _currentUser.UserId,
        };

        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync(cancellationToken);

        await InvitationEmail.SendAsync(_email, _links, invitation.Email, raw, cancellationToken);
        await _audit.LogAsync("InvitationCreated", _currentUser.UserId, "Invitation", invitation.Id);

        return new InviteResult(invitation.Id);
    }
}

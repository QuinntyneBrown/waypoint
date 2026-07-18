using MediatR;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Authentication.Invitations;

public class BulkInviteCommandHandler : IRequestHandler<BulkInviteCommand, BulkInviteResult>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public BulkInviteCommandHandler(
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

    public async Task<BulkInviteResult> Handle(BulkInviteCommand request, CancellationToken cancellationToken)
    {
        if (!Roles.IsValid(request.Role))
        {
            throw new ForbiddenException("Unknown role.");
        }

        if (_currentUser.Role == Roles.Mentor && request.Role != Roles.Youth)
        {
            throw new ForbiddenException("Mentors may only invite youth.");
        }

        var now = _clock.UtcNow;
        var pending = new List<(Invitation Invitation, string RawToken)>();

        foreach (var address in request.Emails
                     .Select(e => e.Trim().ToLowerInvariant())
                     .Where(e => e.Length > 0)
                     .Distinct())
        {
            var (raw, hash) = _tokens.Create();
            var invitation = new Invitation
            {
                Email = address,
                Role = request.Role,
                TokenHash = hash,
                Status = InvitationStatus.Pending,
                CreatedAt = now,
                ExpiresAt = now.AddDays(InvitationPolicy.ExpiryDays),
                CreatedByUserId = _currentUser.UserId,
            };
            _db.Invitations.Add(invitation);
            pending.Add((invitation, raw));
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var (invitation, raw) in pending)
        {
            await InvitationEmail.SendAsync(_email, invitation.Email, raw, cancellationToken);
        }

        await _audit.LogAsync("InvitationsBulkCreated", _currentUser.UserId, "Invitation", details: $"count={pending.Count}");
        return new BulkInviteResult(pending.Count);
    }
}

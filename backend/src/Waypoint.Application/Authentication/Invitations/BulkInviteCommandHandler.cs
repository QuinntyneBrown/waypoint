using MediatR;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Authentication.Invitations;

public class BulkInviteCommandHandler : IRequestHandler<BulkInviteCommand, BulkInviteResult>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationLinkBuilder _links;

    public BulkInviteCommandHandler(
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

    public async Task<BulkInviteResult> Handle(BulkInviteCommand request, CancellationToken cancellationToken)
    {
        if (!Roles.IsValid(request.Role))
        {
            throw new ForbiddenException("Unknown role.");
        }

        var scope = await InvitationScope.ValidateAssignmentAsync(
            _db,
            _currentUser,
            request.Role,
            request.CohortId,
            request.MentorId,
            cancellationToken);

        var now = _clock.UtcNow;
        var pending = new List<(Invitation Invitation, string RawToken)>();
        var addresses = request.Emails
            .Select(InvitationAccountGuard.NormalizeEmail)
            .Where(email => email.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        // Validate the full batch before adding anything so a mixed batch cannot partially succeed.
        await InvitationAccountGuard.EnsureAllAvailableAsync(_db, addresses, cancellationToken);

        foreach (var address in addresses)
        {
            var (raw, hash) = _tokens.Create();
            var invitation = new Invitation
            {
                Email = address,
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
            pending.Add((invitation, raw));
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var (invitation, raw) in pending)
        {
            await InvitationEmail.SendAsync(_email, _links, invitation.Email, raw, cancellationToken);
        }

        await _audit.LogAsync("InvitationsBulkCreated", _currentUser.UserId, "Invitation", details: $"count={pending.Count}");
        return new BulkInviteResult(pending.Count);
    }
}

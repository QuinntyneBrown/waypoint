using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.Authentication.Invitations;

public sealed record RequestReplacementInvitationCommand(string Token)
    : IRequest, IAtomicOutboxRequest;

public sealed class RequestReplacementInvitationCommandValidator
    : AbstractValidator<RequestReplacementInvitationCommand>
{
    public RequestReplacementInvitationCommandValidator() =>
        RuleFor(item => item.Token).NotEmpty().MaximumLength(1024);
}

public sealed class RequestReplacementInvitationCommandHandler
    : IRequestHandler<RequestReplacementInvitationCommand>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;

    public RequestReplacementInvitationCommandHandler(
        IAppDbContext db,
        ISecureTokens tokens,
        IClock clock,
        IEmailSender email,
        IAuditLogger audit)
    {
        _db = db;
        _tokens = tokens;
        _clock = clock;
        _email = email;
        _audit = audit;
    }

    public async Task Handle(RequestReplacementInvitationCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokens.Hash(request.Token);
        var invitation = await _db.Invitations.FirstOrDefaultAsync(
            item => item.TokenHash == hash,
            cancellationToken);
        // Enumeration-safe: unknown tokens have the same successful HTTP response and no observable detail.
        if (invitation is null || invitation.Status == InvitationStatus.Accepted) return;

        // If the address acquired an account after issuance, suppress replacement activity. The
        // public response remains indistinguishable from the unknown-token response.
        if (await InvitationAccountGuard.AccountExistsAsync(_db, invitation.Email, cancellationToken)) return;

        var now = _clock.UtcNow;
        // Suppress repeat mail for the same link for 24 hours, in addition to the endpoint rate limit.
        if (invitation.ReplacementRequestedAt is DateTime last && last > now.AddHours(-24)) return;
        invitation.ReplacementRequestedAt = now;
        invitation.ConcurrencyStamp = Guid.NewGuid();

        var recipients = invitation.CreatedByUserId is Guid creatorId
            ? await _db.Users.AsNoTracking()
                .Where(item => item.Id == creatorId && item.Status == UserStatus.Active)
                .ToListAsync(cancellationToken)
            : [];
        if (recipients.Count == 0)
        {
            recipients = await _db.Users.AsNoTracking()
                .Where(item => item.Role == Roles.ProgramAdministrator && item.Status == UserStatus.Active)
                .ToListAsync(cancellationToken);
        }
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Preserve the endpoint's enumeration-safe, always-accepted contract.
            return;
        }
        foreach (var recipient in recipients.DistinctBy(item => item.Id))
        {
            await _email.SendAsync(
                recipient.Email,
                "Waypoint invitation replacement requested",
                $"A replacement was requested for invitation {invitation.Id}. Review pending invitations in Waypoint; the original token was not included.",
                cancellationToken,
                recipientUserId: recipient.Id);
        }
        await _audit.LogAsync(
            "InvitationReplacementRequested",
            targetType: "Invitation",
            targetId: invitation.Id,
            cancellationToken: cancellationToken);
    }
}

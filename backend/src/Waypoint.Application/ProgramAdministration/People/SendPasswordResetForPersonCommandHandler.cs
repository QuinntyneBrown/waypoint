using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ProgramAdministration.People;

/// <summary>
/// Admin-initiated password reset for a specific person: mints a single-use reset token, emails it,
/// and audits the action (L2-ADMIN-002 AC5).
/// </summary>
public class SendPasswordResetForPersonCommandHandler : IRequestHandler<SendPasswordResetForPersonCommand>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationLinkBuilder _links;

    public SendPasswordResetForPersonCommandHandler(
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

    public async Task Handle(SendPasswordResetForPersonCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");
        if (user.Status == UserStatus.Removed)
        {
            throw new ConflictException("A removed person cannot receive a password reset.");
        }

        var (raw, hash) = _tokens.Create();
        var now = _clock.UtcNow;
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = now.AddHours(1),
            CreatedAt = now,
        });

        await _db.SaveChangesAsync(cancellationToken);

        await _email.SendAsync(
            user.Email,
            "Reset your Waypoint password",
            $"An administrator has initiated a password reset for your Waypoint account. Use this link: {_links.Portal($"/auth/reset-password?token={Uri.EscapeDataString(raw)}")}",
            cancellationToken,
            recipientUserId: user.Id);

        await _audit.LogAsync("PasswordResetSent", _currentUser.UserId, "User", user.Id);
    }
}

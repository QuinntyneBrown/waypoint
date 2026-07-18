using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.People;

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

    public SendPasswordResetForPersonCommandHandler(
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
            "Reset your Word Up password",
            $"An administrator has initiated a password reset for your Word Up account. Use this link: token={raw}",
            cancellationToken);

        await _audit.LogAsync("PasswordResetSent", _currentUser.UserId, "User", user.Id);
    }
}

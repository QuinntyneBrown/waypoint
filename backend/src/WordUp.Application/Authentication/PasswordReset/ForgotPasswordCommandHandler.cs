using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Authentication.PasswordReset;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;

    public ForgotPasswordCommandHandler(
        IAppDbContext db,
        ISecureTokens tokens,
        IClock clock,
        IEmailSender email)
    {
        _db = db;
        _tokens = tokens;
        _clock = clock;
        _email = email;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Always complete normally, whether or not the account exists (L2-AUTH-005 AC1, no enumeration).
        if (user is null)
        {
            return;
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
            $"We received a request to reset your Word Up password. Use this link: token={raw}",
            cancellationToken);
    }
}

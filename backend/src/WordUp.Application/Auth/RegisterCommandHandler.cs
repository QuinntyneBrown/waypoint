using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.Auth;

// NOTE: self-service registration is a development/seed convenience. Real provisioning is invite-only
// (L1-AUTH-003); see the invitation flow. Registered users default to the Youth role, Active status.
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;
    private readonly ISessionIssuer _session;

    public RegisterCommandHandler(IAppDbContext db, IPasswordHasher hasher, IClock clock, ISessionIssuer session)
    {
        _db = db;
        _hasher = hasher;
        _clock = clock;
        _session = session;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new EmailAlreadyRegisteredException(email);
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CreatedAt = _clock.UtcNow,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return await _session.IssueAsync(user, cancellationToken);
    }
}

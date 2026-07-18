using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.Profile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public UpdateProfileCommandHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // Only the editable profile fields; placement (cohort/mentor/guardian) stays read-only.
        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.Pronouns = request.Pronouns;
        user.PhotoUrl = request.PhotoUrl;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ProfileUpdated", userId, "User", userId, cancellationToken: cancellationToken);
    }
}

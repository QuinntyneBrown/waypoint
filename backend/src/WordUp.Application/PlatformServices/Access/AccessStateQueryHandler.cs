using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.Access;

public class AccessStateQueryHandler : IRequestHandler<AccessStateQuery, AccessState>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AccessStateQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AccessState> Handle(AccessStateQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new NotFoundException("User not found.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var hasCohort = user.CohortId != null;
        var consentRequired = !await _db.GuardianConsents
            .AnyAsync(c => c.YouthUserId == userId, cancellationToken);
        var participationAllowed = hasCohort && !consentRequired;

        return new AccessState(hasCohort, consentRequired, participationAllowed);
    }
}

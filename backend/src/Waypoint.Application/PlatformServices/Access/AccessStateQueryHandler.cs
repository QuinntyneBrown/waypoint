using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;
using Waypoint.Application.ConsentAndSafeguarding;

namespace Waypoint.Application.PlatformServices.Access;

public class AccessStateQueryHandler : IRequestHandler<AccessStateQuery, AccessState>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public AccessStateQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<AccessState> Handle(AccessStateQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new NotFoundException("User not found.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var hasCohort = user.CohortId != null;
        var participationConsentAllowed = await ConsentPolicyEvaluator.IsYouthParticipationAllowedAsync(
            _db,
            userId,
            user.GuardianUserId,
            _clock.UtcNow,
            cancellationToken);
        var consentRequired = !participationConsentAllowed;
        var participationAllowed = hasCohort && !consentRequired;

        return new AccessState(hasCohort, consentRequired, participationAllowed);
    }
}

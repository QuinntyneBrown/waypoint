using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;
using WordUp.Application.ConsentAndSafeguarding;

namespace WordUp.Application.PlatformServices.Access;

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
        var policy = await ConsentPolicyEvaluator.LoadAsync(
            _db,
            _clock.UtcNow,
            cancellationToken);
        var consents = await _db.GuardianConsents
            .AsNoTracking()
            .Where(consent => consent.YouthUserId == userId)
            .ToListAsync(cancellationToken);
        var consentRequired = !consents.Any(consent =>
            policy.IsParticipationValid(consent, _clock.UtcNow));
        var participationAllowed = hasCohort && !consentRequired;

        return new AccessState(hasCohort, consentRequired, participationAllowed);
    }
}

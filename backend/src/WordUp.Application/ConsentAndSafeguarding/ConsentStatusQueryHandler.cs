using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ConsentAndSafeguarding;

public class ConsentStatusQueryHandler : IRequestHandler<ConsentStatusQuery, ConsentStatusResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public ConsentStatusQueryHandler(IAppDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<ConsentStatusResult> Handle(ConsentStatusQuery request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // A youth with no guardian on record is treated as an adult and is not gated.
        if (youth.GuardianUserId is null)
        {
            return new ConsentStatusResult(true);
        }

        // Minor: participation is allowed only while a valid guardian consent exists (server-side per-request gate).
        var now = _clock.UtcNow;
        var consents = await _db.GuardianConsents
            .Where(c => c.YouthUserId == youth.Id)
            .ToListAsync(cancellationToken);

        var allowed = consents.Any(c => c.IsParticipationValid(now));
        return new ConsentStatusResult(allowed);
    }
}

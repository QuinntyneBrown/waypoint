using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.People;

public class GetPersonQueryHandler : IRequestHandler<GetPersonQuery, PersonDetail>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public GetPersonQueryHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<PersonDetail> Handle(GetPersonQuery request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");

        var now = _clock.UtcNow;
        var consents = await _db.GuardianConsents
            .Where(c => c.YouthUserId == user.Id)
            .ToListAsync(cancellationToken);
        var hasValidConsent = consents.Any(c => c.IsParticipationValid(now));

        return new PersonDetail(
            user.Id,
            user.DisplayName,
            user.Email,
            user.Role,
            user.Status.ToString(),
            user.CohortId,
            user.MentorId,
            user.GuardianUserId,
            hasValidConsent);
    }
}

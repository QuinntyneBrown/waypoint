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

        var allowed = await ConsentPolicyEvaluator.IsYouthParticipationAllowedAsync(
            _db,
            youth.Id,
            youth.GuardianUserId,
            _clock.UtcNow,
            cancellationToken);
        return new ConsentStatusResult(allowed);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Authentication.Onboarding;

public class OnboardingStatusQueryHandler : IRequestHandler<OnboardingStatusQuery, OnboardingStatusResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public OnboardingStatusQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<OnboardingStatusResult> Handle(OnboardingStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return new OnboardingStatusResult(
            user.DisplayName, user.Pronouns, user.CohortId, user.MentorId, user.OnboardingCompleted);
    }
}

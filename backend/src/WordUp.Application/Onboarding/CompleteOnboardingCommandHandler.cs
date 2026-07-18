using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.Onboarding;

public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public CompleteOnboardingCommandHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // Cohort and mentor are pre-assigned and read-only during onboarding (L2-AUTH-011 AC2) —
        // they are not accepted from the request.
        user.DisplayName = request.DisplayName.Trim();
        user.Pronouns = string.IsNullOrWhiteSpace(request.Pronouns) ? null : request.Pronouns.Trim();
        user.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();
        user.OnboardingCompleted = true;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("OnboardingCompleted", userId, "User", userId);
    }
}

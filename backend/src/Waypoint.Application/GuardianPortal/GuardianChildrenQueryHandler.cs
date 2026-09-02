using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.GuardianPortal;

public sealed class GuardianChildrenQueryHandler
    : IRequestHandler<GuardianChildrenQuery, IReadOnlyList<GuardianChildSummary>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GuardianChildrenQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GuardianChildSummary>> Handle(
        GuardianChildrenQuery request,
        CancellationToken cancellationToken)
    {
        var guardianId = _currentUser.UserId ?? throw new InvalidOperationException("Guardian identity is required.");
        return await _db.Users
            .Where(user => user.GuardianUserId == guardianId && user.Role == Roles.Youth)
            .OrderBy(user => user.DisplayName)
            .Select(user => new GuardianChildSummary(user.Id, user.DisplayName))
            .ToListAsync(cancellationToken);
    }
}

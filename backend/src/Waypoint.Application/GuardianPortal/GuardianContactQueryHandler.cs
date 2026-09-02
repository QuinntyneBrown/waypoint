using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.GuardianPortal;

public sealed class GuardianContactQueryHandler : IRequestHandler<GuardianContactQuery, GuardianContactView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GuardianContactQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GuardianContactView> Handle(
        GuardianContactQuery request,
        CancellationToken cancellationToken)
    {
        var guardianId = _currentUser.UserId ?? throw new ForbiddenException();
        var guardian = await _db.Users.FirstOrDefaultAsync(
                user => user.Id == guardianId && user.Role == Roles.Guardian,
                cancellationToken)
            ?? throw new NotFoundException("Guardian not found.");
        var profile = await _db.GuardianProfiles.FirstOrDefaultAsync(
            candidate => candidate.GuardianUserId == guardianId,
            cancellationToken);

        return new GuardianContactView(
            guardian.DisplayName,
            guardian.Email,
            profile?.Relationship,
            profile?.Phone,
            profile?.EmergencyContactName,
            profile?.EmergencyContactPhone);
    }
}

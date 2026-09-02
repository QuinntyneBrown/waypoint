using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.GuardianPortal;

public class UpdateGuardianContactCommandHandler : IRequestHandler<UpdateGuardianContactCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public UpdateGuardianContactCommandHandler(
        IAppDbContext db, ICurrentUser currentUser, IClock clock, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
    }

    public async Task Handle(UpdateGuardianContactCommand request, CancellationToken cancellationToken)
    {
        // A guardian may only edit their OWN contact record (L2-GUARDIAN-003 AC4 / L2-AUTH-010).
        var guardianId = _currentUser.UserId ?? throw new ForbiddenException();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == guardianId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // Name is rendered in the portal, so encode stored free text (L2-NFR-001). Email is validated
        // structurally and stored as-is.
        user.DisplayName = WebUtility.HtmlEncode(request.DisplayName);
        user.Email = request.Email;

        var profile = await _db.GuardianProfiles
            .FirstOrDefaultAsync(p => p.GuardianUserId == guardianId, cancellationToken);
        if (profile is null)
        {
            profile = new GuardianProfile { GuardianUserId = guardianId };
            _db.GuardianProfiles.Add(profile);
        }

        // All rendered free-text fields are encoded before persistence (L2-NFR-001).
        profile.Relationship = WebUtility.HtmlEncode(request.Relationship);
        profile.Phone = WebUtility.HtmlEncode(request.Phone);
        profile.EmergencyContactName = WebUtility.HtmlEncode(request.EmergencyContactName);
        profile.EmergencyContactPhone = WebUtility.HtmlEncode(request.EmergencyContactPhone);
        profile.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("GuardianContactUpdated", guardianId, "GuardianProfile", profile.Id,
            cancellationToken: cancellationToken);
    }
}

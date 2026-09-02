using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.ProgramAdministration.Settings;

public class GetOrgSettingsQueryHandler : IRequestHandler<GetOrgSettingsQuery, OrgSettingsDto>
{
    private readonly IAppDbContext _db;

    public GetOrgSettingsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<OrgSettingsDto> Handle(GetOrgSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _db.OrgSettings.FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            // Materialize the singleton default so first-read and subsequent updates share one row (L2-ADMIN-006).
            settings = new OrgSettings();
            _db.OrgSettings.Add(settings);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new OrgSettingsDto(
            settings.Name,
            settings.DisplayName,
            settings.LogoUrl,
            settings.City,
            settings.ContactEmail,
            settings.ConsentStatement,
            settings.RequireReConsentEachTerm,
            settings.RequireMediaConsent);
    }
}

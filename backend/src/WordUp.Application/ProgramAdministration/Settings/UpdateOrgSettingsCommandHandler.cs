using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ProgramAdministration.Settings;

public class UpdateOrgSettingsCommandHandler : IRequestHandler<UpdateOrgSettingsCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public UpdateOrgSettingsCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateOrgSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _db.OrgSettings.FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            settings = new OrgSettings();
            _db.OrgSettings.Add(settings);
        }

        settings.Name = request.Name.Trim();
        settings.DisplayName = request.DisplayName;
        settings.LogoUrl = request.LogoUrl;
        settings.City = request.City;
        settings.ContactEmail = request.ContactEmail;
        settings.ConsentStatement = request.ConsentStatement;
        settings.RequireReConsentEachTerm = request.RequireReConsentEachTerm;
        settings.RequireMediaConsent = request.RequireMediaConsent;

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("OrgSettingsUpdated", _currentUser.UserId, "OrgSettings", settings.Id);
    }
}

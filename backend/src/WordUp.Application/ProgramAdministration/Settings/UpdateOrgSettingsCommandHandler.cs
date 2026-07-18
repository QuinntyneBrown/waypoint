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
        settings.DisplayName = Clean(request.DisplayName);
        settings.LogoUrl = Clean(request.LogoUrl);
        settings.City = Clean(request.City);
        settings.ContactEmail = Clean(request.ContactEmail)?.ToLowerInvariant();
        settings.ConsentStatement = Clean(request.ConsentStatement);
        settings.RequireReConsentEachTerm = request.RequireReConsentEachTerm;
        settings.RequireMediaConsent = request.RequireMediaConsent;

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "OrgSettingsUpdated",
            _currentUser.UserId,
            "OrgSettings",
            settings.Id,
            cancellationToken: cancellationToken);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

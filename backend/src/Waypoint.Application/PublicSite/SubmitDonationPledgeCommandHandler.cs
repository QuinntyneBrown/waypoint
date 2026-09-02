using System.Net;
using MediatR;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Security;

namespace Waypoint.Application.PublicSite;

public class SubmitDonationPledgeCommandHandler : IRequestHandler<SubmitDonationPledgeCommand, SubmitDonationPledgeResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly AuthenticatedPrivacyWriteGuard _privacyWriteGuard;

    public SubmitDonationPledgeCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        AuthenticatedPrivacyWriteGuard privacyWriteGuard)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _privacyWriteGuard = privacyWriteGuard;
    }

    public async Task<SubmitDonationPledgeResult> Handle(SubmitDonationPledgeCommand request, CancellationToken cancellationToken)
    {
        var authenticatedAuthorId = await _privacyWriteGuard.AcquireAuthorAsync(cancellationToken);

        // Persist ONLY the pledge intent + an optional opaque provider reference — never any card/PAN data
        // (L2-PUB-003 AC3). Free-text is HTML-encoded before persistence (L2-NFR-001).
        var pledge = new DonationPledge
        {
            OwnerUserId = authenticatedAuthorId,
            DonorName = WebUtility.HtmlEncode(request.DonorName.Trim()),
            Email = request.Email.Trim().ToLowerInvariant(),
            Tier = WebUtility.HtmlEncode(request.Tier.Trim()),
            MonthlyAmount = request.MonthlyAmount,
            Frequency = string.IsNullOrWhiteSpace(request.Frequency) ? "monthly" : request.Frequency.Trim().ToLowerInvariant(),
            ProviderReference = string.IsNullOrWhiteSpace(request.ProviderReference) ? null : request.ProviderReference.Trim(),
            CreatedAt = _clock.UtcNow,
        };

        _db.DonationPledges.Add(pledge);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "DonationPledgeSubmitted",
            pledge.OwnerUserId,
            "DonationPledge",
            pledge.Id);

        return new SubmitDonationPledgeResult(
            pledge.Id,
            "Thank you for your generosity — your monthly pledge helps a young leader flourish.");
    }
}

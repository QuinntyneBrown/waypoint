using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ConsentAndSafeguarding;

public sealed class RequestedConsentQueryHandler
    : IRequestHandler<RequestedConsentQuery, RequestedConsentView>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;

    public RequestedConsentQueryHandler(IAppDbContext db, ISecureTokens tokens, IClock clock)
    {
        _db = db;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<RequestedConsentView> Handle(
        RequestedConsentQuery request,
        CancellationToken cancellationToken)
    {
        var tokenHash = _tokens.Hash(request.Token);
        var consentRequest = await _db.GuardianConsentRequests.FirstOrDefaultAsync(
            item => item.TokenHash == tokenHash,
            cancellationToken);
        if (consentRequest is null || !consentRequest.IsUsable(_clock.UtcNow))
        {
            throw new ConsentRequestLinkInvalidException();
        }

        var youth = await _db.Users.FirstOrDefaultAsync(
            user => user.Id == consentRequest.YouthUserId,
            cancellationToken);
        if (youth is null)
        {
            throw new ConsentRequestLinkInvalidException();
        }
        var policy = await ConsentPolicyEvaluator.LoadAsync(
            _db,
            _clock.UtcNow,
            cancellationToken);

        return new RequestedConsentView(
            youth.DisplayName,
            RequestGuardianConsentCommandHandler.MaskEmail(consentRequest.Destination),
            consentRequest.ExpiresAt,
            policy.OrganizationName,
            policy.ContactEmail,
            policy.ConsentStatement,
            policy.RequireMediaConsent);
    }
}

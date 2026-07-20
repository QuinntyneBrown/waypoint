using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ConsentAndSafeguarding;

public sealed class RequestGuardianConsentCommandHandler
    : IRequestHandler<RequestGuardianConsentCommand, ConsentDeliveryResult>
{
    private const int ExpiryDays = 7;
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;
    private readonly IApplicationLinkBuilder _links;

    public RequestGuardianConsentCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        ISecureTokens tokens,
        IClock clock,
        IEmailSender email,
        IAuditLogger audit,
        IApplicationLinkBuilder links)
    {
        _db = db;
        _currentUser = currentUser;
        _tokens = tokens;
        _clock = clock;
        _email = email;
        _audit = audit;
        _links = links;
    }

    public async Task<ConsentDeliveryResult> Handle(
        RequestGuardianConsentCommand request,
        CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new NotFoundException("Youth not found.");
        var youth = await _db.Users.FirstOrDefaultAsync(
            user => user.Id == youthId && user.Role == Roles.Youth,
            cancellationToken) ?? throw new NotFoundException("Youth not found.");

        if (youth.GuardianUserId is null)
        {
            throw new GuardianContactMissingException();
        }

        var guardian = await _db.Users.FirstOrDefaultAsync(
            user => user.Id == youth.GuardianUserId &&
                    user.Role == Roles.Guardian &&
                    user.Status == UserStatus.Active,
            cancellationToken);
        if (guardian is null || string.IsNullOrWhiteSpace(guardian.Email))
        {
            throw new GuardianContactMissingException();
        }

        var now = _clock.UtcNow;
        var previous = await _db.GuardianConsentRequests
            .Where(item => item.YouthUserId == youth.Id && item.UsedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var item in previous)
        {
            item.UsedAt = now;
        }

        var (rawToken, tokenHash) = _tokens.Create();
        var consentRequest = new GuardianConsentRequest
        {
            YouthUserId = youth.Id,
            GuardianUserId = guardian.Id,
            Destination = guardian.Email,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(ExpiryDays),
        };
        _db.GuardianConsentRequests.Add(consentRequest);
        await _db.SaveChangesAsync(cancellationToken);

        await _email.SendAsync(
            guardian.Email,
            $"Consent requested for {youth.DisplayName}",
            $"{youth.DisplayName} needs your participation consent for Word Up. " +
            $"Review and sign the secure form: {_links.Portal($"/consent/respond?token={Uri.EscapeDataString(rawToken)}")} " +
            $"This one-time link expires {consentRequest.ExpiresAt:yyyy-MM-dd}.",
            cancellationToken,
            subjectUserId: youth.Id,
            recipientUserId: guardian.Id);
        await _audit.LogAsync(
            "GuardianConsentRequested",
            youth.Id,
            "GuardianConsentRequest",
            consentRequest.Id,
            cancellationToken: cancellationToken);

        return new ConsentDeliveryResult("email", MaskEmail(guardian.Email));
    }

    internal static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 0 || at == email.Length - 1)
        {
            return "***";
        }

        return $"{email[0]}***{email[at..]}";
    }
}

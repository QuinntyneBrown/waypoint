using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Authentication.Invitations;

public sealed record InvitationContextQuery(string Token) : IRequest<InvitationContext>;

public sealed record InvitationContext(
    string State,
    string? Role,
    string? CohortName,
    string? MentorName,
    DateTime? ExpiresAt,
    string? MaskedEmail);

public sealed class InvitationContextQueryHandler
    : IRequestHandler<InvitationContextQuery, InvitationContext>
{
    private readonly IAppDbContext _db;
    private readonly ISecureTokens _tokens;
    private readonly IClock _clock;

    public InvitationContextQueryHandler(IAppDbContext db, ISecureTokens tokens, IClock clock)
    {
        _db = db;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<InvitationContext> Handle(
        InvitationContextQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token)) return Invalid();
        var hash = _tokens.Hash(request.Token);
        var invitation = await _db.Invitations.AsNoTracking()
            .FirstOrDefaultAsync(item => item.TokenHash == hash, cancellationToken);
        if (invitation is null) return Invalid();

        var cohortName = invitation.CohortId is Guid cohortId
            ? await _db.Cohorts.AsNoTracking().Where(item => item.Id == cohortId)
                .Select(item => item.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        var mentorName = invitation.MentorId is Guid mentorId
            ? await _db.Users.AsNoTracking().Where(item => item.Id == mentorId)
                .Select(item => item.DisplayName).FirstOrDefaultAsync(cancellationToken)
            : null;
        var state = invitation.Status switch
        {
            InvitationStatus.Accepted => "accepted",
            InvitationStatus.Revoked => "revoked",
            _ when invitation.ExpiresAt <= _clock.UtcNow => "expired",
            _ => "valid",
        };
        return new InvitationContext(
            state,
            invitation.Role,
            cohortName,
            mentorName,
            invitation.ExpiresAt,
            Mask(invitation.Email));
    }

    private static InvitationContext Invalid() => new("invalid", null, null, null, null, null);

    private static string Mask(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 0) return "***";
        var local = email[..at];
        var visible = local[..1];
        return $"{visible}***{email[at..]}";
    }
}

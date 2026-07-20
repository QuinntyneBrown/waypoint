using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.OperationalAdministration;

public sealed record ListInvitationsQuery : IRequest<IReadOnlyList<OperationalInvitationResult>>;

public sealed record OperationalInvitationResult(
    Guid Id,
    string Email,
    string Role,
    string Status,
    Guid? CohortId,
    Guid? MentorId,
    DateTime CreatedAt,
    DateTime ExpiresAt);

public sealed class ListInvitationsQueryHandler
    : IRequestHandler<ListInvitationsQuery, IReadOnlyList<OperationalInvitationResult>>
{
    private readonly IAppDbContext _db;

    public ListInvitationsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<OperationalInvitationResult>> Handle(
        ListInvitationsQuery request,
        CancellationToken cancellationToken) =>
        await _db.Invitations
            .AsNoTracking()
            .OrderByDescending(invitation => invitation.CreatedAt)
            .Select(invitation => new OperationalInvitationResult(
                invitation.Id,
                invitation.Email,
                invitation.Role,
                invitation.Status.ToString(),
                invitation.CohortId,
                invitation.MentorId,
                invitation.CreatedAt,
                invitation.ExpiresAt))
            .ToListAsync(cancellationToken);
}

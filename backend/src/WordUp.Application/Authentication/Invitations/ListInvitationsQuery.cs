using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Authentication.Invitations;

public sealed record InvitationListQuery(
    string? Status,
    string? Role,
    Guid? CohortId,
    string? Search,
    int Page = 1,
    int PageSize = 25) : IRequest<InvitationPage>;

public sealed record InvitationPage(
    IReadOnlyList<InvitationListItem> Items,
    int Page,
    int PageSize,
    int Total);

public sealed record InvitationListItem(
    Guid Id,
    string Email,
    string Role,
    Guid? CohortId,
    string? CohortName,
    Guid? MentorId,
    string? MentorName,
    string Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    string? CreatedByName);

public sealed class InvitationListQueryHandler
    : IRequestHandler<InvitationListQuery, InvitationPage>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public InvitationListQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<InvitationPage> Handle(
        InvitationListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = request.PageSize is < 1 or > 100 ? 25 : request.PageSize;
        var query = InvitationScope.Apply(_db.Invitations.AsNoTracking(), _currentUser);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var requestedStatus = request.Status.Trim();
            if (requestedStatus.Equals("Expired", StringComparison.OrdinalIgnoreCase))
            {
                var now = _clock.UtcNow;
                query = query.Where(item => item.Status == InvitationStatus.Pending && item.ExpiresAt <= now);
            }
            else if (requestedStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                var now = _clock.UtcNow;
                query = query.Where(item => item.Status == InvitationStatus.Pending && item.ExpiresAt > now);
            }
            else
            {
                if (!Enum.TryParse<InvitationStatus>(requestedStatus, true, out var status))
                    return new InvitationPage([], page, pageSize, 0);
                query = query.Where(item => item.Status == status);
            }
        }
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var role = request.Role.Trim();
            query = query.Where(item => item.Role == role);
        }
        if (request.CohortId is Guid cohortId)
            query = query.Where(item => item.CohortId == cohortId);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(item => item.Email.ToLower().Contains(search));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(item => item.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var cohortIds = rows.Where(item => item.CohortId != null).Select(item => item.CohortId!.Value).Distinct().ToList();
        var userIds = rows.SelectMany(item => new[] { item.MentorId, item.CreatedByUserId })
            .Where(item => item != null).Select(item => item!.Value).Distinct().ToList();
        var cohorts = await _db.Cohorts.AsNoTracking()
            .Where(item => cohortIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var users = await _db.Users.AsNoTracking()
            .Where(item => userIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.DisplayName, cancellationToken);

        var items = rows.Select(item => new InvitationListItem(
            item.Id,
            item.Email,
            item.Role,
            item.CohortId,
            item.CohortId is Guid cid && cohorts.TryGetValue(cid, out var cohortName) ? cohortName : null,
            item.MentorId,
            item.MentorId is Guid mid && users.TryGetValue(mid, out var mentorName) ? mentorName : null,
            item.Status == InvitationStatus.Pending && item.ExpiresAt <= _clock.UtcNow
                ? "Expired"
                : item.Status.ToString(),
            item.CreatedAt,
            item.ExpiresAt,
            item.CreatedByUserId is Guid creatorId && users.TryGetValue(creatorId, out var creatorName) ? creatorName : null))
            .ToList();
        return new InvitationPage(items, page, pageSize, total);
    }
}

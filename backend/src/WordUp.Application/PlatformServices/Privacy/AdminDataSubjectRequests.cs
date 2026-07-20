using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.PlatformServices.Privacy;

public record AdminDataSubjectRequestsQuery(string? Status, int Page = 1, int PageSize = 50)
    : IRequest<AdminDataSubjectRequestPage>;

public record AdminDataSubjectRequestPage(
    IReadOnlyList<AdminDataSubjectRequestItem> Items,
    int Page,
    int PageSize,
    int Total);

public record AdminDataSubjectRequestItem(
    Guid Id,
    string Reference,
    Guid SubjectUserId,
    string SubjectName,
    string SubjectEmail,
    string ActionType,
    string? Details,
    DataSubjectCorrection? Correction,
    string Status,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    Guid? CompletedByUserId,
    string? ResolutionNote);

public sealed class AdminDataSubjectRequestsQueryHandler
    : IRequestHandler<AdminDataSubjectRequestsQuery, AdminDataSubjectRequestPage>
{
    private readonly IAppDbContext _db;

    public AdminDataSubjectRequestsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<AdminDataSubjectRequestPage> Handle(
        AdminDataSubjectRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;
        var query = _db.DataSubjectRequests.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<DataSubjectRequestStatus>(request.Status, true, out var status))
            {
                return new AdminDataSubjectRequestPage([], page, pageSize, 0);
            }
            query = query.Where(item => item.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(item => item.Status)
            .ThenBy(item => item.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var subjectIds = rows.Select(item => item.SubjectUserId).Distinct().ToList();
        var subjects = await _db.Users.AsNoTracking()
            .Where(item => subjectIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var items = rows.Select(item =>
        {
            subjects.TryGetValue(item.SubjectUserId, out var subject);
            return new AdminDataSubjectRequestItem(
                item.Id,
                item.Reference,
                item.SubjectUserId,
                subject?.DisplayName ?? "Unknown subject",
                subject?.Email ?? "unavailable",
                item.ActionType,
                item.Details,
                item.ActionType == "correction"
                    ? new DataSubjectCorrection(
                        item.RequestedDisplayName,
                        item.RequestedEmail,
                        item.RequestedPronouns,
                        item.RequestedClearPronouns)
                    : null,
                item.Status.ToString(),
                item.RequestedAt,
                item.CompletedAt,
                item.CompletedByUserId,
                item.ResolutionNote);
        }).ToList();
        return new AdminDataSubjectRequestPage(items, page, pageSize, total);
    }
}

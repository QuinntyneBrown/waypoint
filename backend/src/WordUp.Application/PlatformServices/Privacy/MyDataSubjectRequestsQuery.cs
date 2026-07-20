using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.Privacy;

public sealed record MyDataSubjectRequestsQuery(int Page = 1, int PageSize = 25)
    : IRequest<MyDataSubjectRequestPage>;

public sealed record MyDataSubjectRequestPage(
    IReadOnlyList<MyDataSubjectRequestItem> Items,
    int Page,
    int PageSize,
    int Total,
    bool CanExport);

public sealed record MyDataSubjectRequestItem(
    Guid Id,
    string Reference,
    string ActionType,
    string? Details,
    DataSubjectCorrection? Correction,
    string Status,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    string? ResolutionNote);

public sealed class MyDataSubjectRequestsQueryHandler
    : IRequestHandler<MyDataSubjectRequestsQuery, MyDataSubjectRequestPage>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public MyDataSubjectRequestsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<MyDataSubjectRequestPage> Handle(
        MyDataSubjectRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var page = Math.Max(1, request.Page);
        var pageSize = request.PageSize is < 1 or > 100 ? 25 : request.PageSize;
        var query = _db.DataSubjectRequests.AsNoTracking()
            .Where(item => item.SubjectUserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var canExport = await query.AnyAsync(item =>
            item.ActionType == "access"
            && item.Status == DataSubjectRequestStatus.Completed,
            cancellationToken);
        var rows = await query.OrderByDescending(item => item.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new MyDataSubjectRequestPage(
            rows.Select(item => new MyDataSubjectRequestItem(
                item.Id,
                item.Reference,
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
                item.ResolutionNote)).ToList(),
            page,
            pageSize,
            total,
            canExport);
    }
}

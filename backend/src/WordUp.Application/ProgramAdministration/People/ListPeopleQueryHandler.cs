using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ProgramAdministration.People;

public class ListPeopleQueryHandler : IRequestHandler<ListPeopleQuery, PagedPeopleResult>
{
    private readonly IAppDbContext _db;

    public ListPeopleQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedPeopleResult> Handle(ListPeopleQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;

        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            query = query.Where(u => u.Role == request.Role);
        }

        if (request.CohortId is { } cohortId)
        {
            query = query.Where(u => u.CohortId == cohortId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.DisplayName.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new PersonSummary(
                u.Id,
                u.DisplayName,
                u.Email,
                u.Role,
                u.Status.ToString(),
                u.CohortId,
                u.MentorId,
                u.LastSignInAt))
            .ToListAsync(cancellationToken);

        return new PagedPeopleResult(items, total, page, pageSize);
    }
}

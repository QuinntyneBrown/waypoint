using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ApplicationIntake;

public class ListApplicationsQueryHandler
    : IRequestHandler<ListApplicationsQuery, IReadOnlyList<ApplicationSummary>>
{
    private readonly IAppDbContext _db;

    public ListApplicationsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ApplicationSummary>> Handle(
        ListApplicationsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<YouthApplication> query = _db.Applications;

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<ApplicationStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        var rows = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.FirstName,
                a.LastName,
                a.Age,
                a.Neighbourhood,
                a.TrackInterest,
                a.Status,
                a.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        // IsMinor + display status are computed in memory (not EF-translatable).
        return rows
            .Select(a => new ApplicationSummary(
                a.Id,
                a.FirstName,
                a.LastName,
                a.Age,
                a.Age < 18,
                a.Neighbourhood,
                a.TrackInterest,
                DisplayStatus(a.Status),
                a.CreatedAt))
            .ToList();
    }

    private static string DisplayStatus(ApplicationStatus status) => status switch
    {
        ApplicationStatus.New => "New",
        ApplicationStatus.InReview => "In review",
        ApplicationStatus.NeedsConsent => "Needs consent",
        ApplicationStatus.Approved => "Approved",
        ApplicationStatus.Declined => "Declined",
        _ => status.ToString(),
    };
}

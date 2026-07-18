using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.Reports;

public record StaffActivityReportQuery() : IRequest<IReadOnlyList<StaffActivityRow>>;

public record StaffActivityRow(
    Guid MentorId,
    int Caseload,
    int CheckIns,
    int Reviews,
    bool Overloaded);

public class StaffActivityReportQueryHandler
    : IRequestHandler<StaffActivityReportQuery, IReadOnlyList<StaffActivityRow>>
{
    private const int OverloadThreshold = 8;

    private readonly IAppDbContext _db;

    public StaffActivityReportQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<StaffActivityRow>> Handle(
        StaffActivityReportQuery request,
        CancellationToken cancellationToken)
    {
        var mentorIds = await _db.Users
            .Where(u => u.Role == Roles.Mentor)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var rows = new List<StaffActivityRow>(mentorIds.Count);
        foreach (var mentorId in mentorIds)
        {
            var caseload = await _db.Users
                .CountAsync(u => u.Role == Roles.Youth && u.MentorId == mentorId, cancellationToken);
            var checkIns = await _db.CheckIns
                .CountAsync(c => c.MentorUserId == mentorId, cancellationToken);
            var reviews = await _db.Submissions
                .CountAsync(s => s.ReviewedByUserId == mentorId, cancellationToken);

            rows.Add(new StaffActivityRow(mentorId, caseload, checkIns, reviews, caseload > OverloadThreshold));
        }

        return rows;
    }
}

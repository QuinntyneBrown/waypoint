using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ReportingAndData.Reports;

public record StaffActivityReportQuery() : IRequest<IReadOnlyList<StaffActivityRow>>;

public record StaffActivityRow(
    Guid MentorId,
    string MentorName,
    string Email,
    int Caseload,
    int CheckIns,
    int Reviews,
    int TargetMinimum,
    int TargetMaximum,
    string LoadBalance,
    bool Overloaded);

public class StaffActivityReportQueryHandler
    : IRequestHandler<StaffActivityReportQuery, IReadOnlyList<StaffActivityRow>>
{
    public const int TargetMinimum = 4;
    public const int TargetMaximum = 8;

    private readonly IAppDbContext _db;

    public StaffActivityReportQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<StaffActivityRow>> Handle(
        StaffActivityReportQuery request,
        CancellationToken cancellationToken)
    {
        var mentors = await _db.Users
            .AsNoTracking()
            .Where(user => user.Role == Roles.Mentor && user.Status == UserStatus.Active)
            .OrderBy(user => user.DisplayName)
            .Select(user => new { user.Id, user.DisplayName, user.Email })
            .ToListAsync(cancellationToken);
        var mentorIds = mentors.Select(mentor => mentor.Id).ToList();
        var caseloads = await _db.Users
            .AsNoTracking()
            .Where(user => user.Role == Roles.Youth
                && user.Status == UserStatus.Active
                && user.MentorId.HasValue
                && mentorIds.Contains(user.MentorId.Value))
            .GroupBy(user => user.MentorId!.Value)
            .Select(group => new { MentorId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.MentorId, row => row.Count, cancellationToken);
        var checkIns = await _db.CheckIns
            .AsNoTracking()
            .Where(item => mentorIds.Contains(item.MentorUserId))
            .GroupBy(item => item.MentorUserId)
            .Select(group => new { MentorId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.MentorId, row => row.Count, cancellationToken);
        var reviews = await _db.Submissions
            .AsNoTracking()
            .Where(item => item.ReviewedByUserId.HasValue
                && mentorIds.Contains(item.ReviewedByUserId.Value))
            .GroupBy(item => item.ReviewedByUserId!.Value)
            .Select(group => new { MentorId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.MentorId, row => row.Count, cancellationToken);

        var rows = new List<StaffActivityRow>(mentors.Count);
        foreach (var mentor in mentors)
        {
            var caseload = caseloads.GetValueOrDefault(mentor.Id);
            var loadBalance = caseload < TargetMinimum
                ? "Under target"
                : caseload > TargetMaximum
                    ? "Overloaded"
                    : "Balanced";
            rows.Add(new StaffActivityRow(
                mentor.Id,
                mentor.DisplayName,
                mentor.Email,
                caseload,
                checkIns.GetValueOrDefault(mentor.Id),
                reviews.GetValueOrDefault(mentor.Id),
                TargetMinimum,
                TargetMaximum,
                loadBalance,
                caseload > TargetMaximum));
        }

        return rows;
    }
}

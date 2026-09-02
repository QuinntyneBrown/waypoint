using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.ReportingAndData.Reports;

public record StaffActivityReportQuery(ReportFilter? Filter = null) : IRequest<IReadOnlyList<StaffActivityRow>>;

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
    private readonly ICurrentUser _currentUser;

    public StaffActivityReportQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<StaffActivityRow>> Handle(
        StaffActivityReportQuery request,
        CancellationToken cancellationToken)
    {
        return await ReportData.StaffAsync(
            _db,
            _currentUser,
            request.Filter ?? new ReportFilter(),
            cancellationToken);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.ReportingAndData.Reports;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.ProgramAdministration.MentorAssignments;

public sealed record MentorAssignmentOverviewQuery() : IRequest<MentorAssignmentOverview>;

public sealed record MentorAssignmentOverview(
    IReadOnlyList<StaffActivityRow> Mentors,
    IReadOnlyList<YouthMentorAssignment> Youth,
    IReadOnlyList<CohortMentorAssignment> Cohorts);

public sealed record YouthMentorAssignment(
    Guid Id,
    string Name,
    string Email,
    Guid? CohortId,
    string? CohortName,
    Guid? MentorId,
    string? MentorName);

public sealed record CohortMentorAssignment(
    Guid Id,
    string Name,
    Guid? LeadMentorId,
    string? LeadMentorName,
    IReadOnlyList<MentorAssignmentOption> SupportingMentors);

public sealed record MentorAssignmentOption(Guid Id, string Name);

public sealed class MentorAssignmentOverviewQueryHandler
    : IRequestHandler<MentorAssignmentOverviewQuery, MentorAssignmentOverview>
{
    private readonly IAppDbContext _db;
    private readonly ISender _sender;

    public MentorAssignmentOverviewQueryHandler(IAppDbContext db, ISender sender)
    {
        _db = db;
        _sender = sender;
    }

    public async Task<MentorAssignmentOverview> Handle(
        MentorAssignmentOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var mentorLoads = await _sender.Send(new StaffActivityReportQuery(), cancellationToken);
        var mentorNames = mentorLoads.ToDictionary(item => item.MentorId, item => item.MentorName);
        var cohortNames = await _db.Cohorts
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var youth = await _db.Users
            .AsNoTracking()
            .Where(item => item.Role == Roles.Youth && item.Status == UserStatus.Active)
            .OrderBy(item => item.DisplayName)
            .Select(item => new
            {
                item.Id,
                item.DisplayName,
                item.Email,
                item.CohortId,
                item.MentorId,
            })
            .ToListAsync(cancellationToken);
        var youthAssignments = youth
            .Select(item => new YouthMentorAssignment(
                item.Id,
                item.DisplayName,
                item.Email,
                item.CohortId,
                item.CohortId is { } cohortId
                    ? cohortNames.GetValueOrDefault(cohortId)
                    : null,
                item.MentorId,
                item.MentorId is { } mentorId
                    ? mentorNames.GetValueOrDefault(mentorId)
                    : null))
            .ToList();

        var cohorts = await _db.Cohorts
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new { item.Id, item.Name, item.MentorId })
            .ToListAsync(cancellationToken);
        var cohortIds = cohorts.Select(item => item.Id).ToList();
        var supportingRows = await _db.CohortMentors
            .AsNoTracking()
            .Where(item => cohortIds.Contains(item.CohortId) && !item.IsLead)
            .Select(item => new { item.CohortId, item.MentorId })
            .ToListAsync(cancellationToken);
        var cohortAssignments = cohorts
            .Select(cohort => new CohortMentorAssignment(
                cohort.Id,
                cohort.Name,
                cohort.MentorId,
                cohort.MentorId is { } leadMentorId
                    ? mentorNames.GetValueOrDefault(leadMentorId)
                    : null,
                supportingRows
                    .Where(item => item.CohortId == cohort.Id
                        && mentorNames.ContainsKey(item.MentorId))
                    .Select(item => new MentorAssignmentOption(
                        item.MentorId,
                        mentorNames[item.MentorId]))
                    .OrderBy(item => item.Name)
                    .ToList()))
            .ToList();

        return new MentorAssignmentOverview(mentorLoads, youthAssignments, cohortAssignments);
    }
}

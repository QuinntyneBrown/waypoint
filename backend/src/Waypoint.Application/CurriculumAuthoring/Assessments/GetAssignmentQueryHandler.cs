using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed class GetAssignmentQueryHandler : IRequestHandler<GetAssignmentQuery, AssignmentDetail>
{
    private readonly IAppDbContext _db;

    public GetAssignmentQueryHandler(IAppDbContext db) => _db = db;

    public async Task<AssignmentDetail> Handle(
        GetAssignmentQuery request,
        CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == request.AssignmentId,
            cancellationToken) ?? throw new NotFoundException("Assignment not found.");
        var rubric = await _db.RubricCriteria.AsNoTracking()
            .Where(item => item.AssignmentId == assignment.Id)
            .OrderBy(item => item.Order)
            .Select(item => new AssignmentRubricDetail(
                item.Id,
                item.Description,
                item.Points,
                item.Order))
            .ToListAsync(cancellationToken);
        return new AssignmentDetail(
            assignment.Id,
            assignment.ModuleId,
            assignment.LessonId,
            assignment.Title,
            assignment.TaskDescription,
            assignment.SubmissionTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            assignment.DueDate,
            assignment.GracePeriodDays,
            assignment.AllowLate,
            assignment.RequireMentorApproval,
            assignment.TotalPoints,
            assignment.Status.ToString(),
            rubric);
    }
}

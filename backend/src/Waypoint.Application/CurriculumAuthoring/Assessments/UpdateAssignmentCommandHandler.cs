using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public UpdateAssignmentCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(
            item => item.Id == request.AssignmentId,
            cancellationToken) ?? throw new NotFoundException("Assignment not found.");
        if (assignment.ModuleId != request.ModuleId || assignment.LessonId != request.LessonId)
        {
            throw new ConflictException("An assignment cannot be moved while editing it.");
        }
        if (await _db.Submissions.AnyAsync(
            item => item.AssignmentId == assignment.Id,
            cancellationToken))
        {
            throw new ConflictException("An assignment with submissions can no longer be edited.");
        }

        assignment.Title = request.Title.Trim();
        assignment.TaskDescription = request.TaskDescription.Trim();
        assignment.SubmissionTypes = string.Join(",", request.SubmissionTypes
            .Select(item => item.Trim().ToLowerInvariant())
            .Distinct());
        assignment.DueDate = request.DueDate;
        assignment.GracePeriodDays = request.GracePeriodDays;
        assignment.AllowLate = request.AllowLate;
        assignment.RequireMentorApproval = request.RequireMentorApproval;
        assignment.TotalPoints = request.Rubric.Sum(item => item.Points);
        var oldRubric = await _db.RubricCriteria
            .Where(item => item.AssignmentId == assignment.Id)
            .ToListAsync(cancellationToken);
        _db.RubricCriteria.RemoveRange(oldRubric);
        for (var index = 0; index < request.Rubric.Count; index++)
        {
            var criterion = request.Rubric[index];
            _db.RubricCriteria.Add(new RubricCriterion
            {
                AssignmentId = assignment.Id,
                Description = criterion.Description.Trim(),
                Points = criterion.Points,
                Order = index,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "AssignmentUpdated",
            _currentUser.UserId,
            "Assignment",
            assignment.Id,
            cancellationToken: cancellationToken);
    }
}

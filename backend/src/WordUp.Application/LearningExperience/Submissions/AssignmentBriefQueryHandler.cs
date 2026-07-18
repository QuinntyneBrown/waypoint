using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.Submissions;

public sealed class AssignmentBriefQueryHandler : IRequestHandler<AssignmentBriefQuery, AssignmentBriefView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AssignmentBriefQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AssignmentBriefView> Handle(
        AssignmentBriefQuery request,
        CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var assignment = await AssignmentAccessGuard.EnsureAccessibleAsync(
            _db, request.AssignmentId, youthId, cancellationToken);
        var rubric = await _db.RubricCriteria.AsNoTracking()
            .Where(item => item.AssignmentId == assignment.Id)
            .OrderBy(item => item.Order)
            .Select(item => new AssignmentRubricView(item.Id, item.Description, item.Points))
            .ToListAsync(cancellationToken);
        var existing = await _db.Submissions.AsNoTracking()
            .Where(item => item.AssignmentId == assignment.Id && item.YouthUserId == youthId)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new AssignmentBriefView(
            assignment.Id,
            assignment.Title,
            assignment.TaskDescription,
            assignment.DueDate,
            AssignmentAccessGuard.SubmissionTypes(assignment),
            rubric,
            assignment.TotalPoints,
            AssignmentUploadPolicy.MaxBytes,
            existing?.Id,
            existing?.Status.ToString());
    }
}

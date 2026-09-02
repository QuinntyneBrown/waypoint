using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed class PublishAssignmentCommandHandler : IRequestHandler<PublishAssignmentCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public PublishAssignmentCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(PublishAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(
            item => item.Id == request.AssignmentId,
            cancellationToken) ?? throw new NotFoundException("Assignment not found.");
        var moduleId = assignment.ModuleId;
        if (moduleId is null && assignment.LessonId is { } lessonId)
        {
            moduleId = await _db.Lessons
                .Where(item => item.Id == lessonId && item.ArchivedAt == null)
                .Select(item => (Guid?)item.ModuleId)
                .FirstOrDefaultAsync(cancellationToken);
        }
        if (moduleId is null)
        {
            throw new ConflictException("Attach the assignment to a module or lesson before publishing.");
        }
        var trackId = await _db.Modules
            .Where(item => item.Id == moduleId)
            .Select(item => (Guid?)item.TrackId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Assignment module not found.");

        assignment.Status = request.Status;
        if (request.Status == PublishStatus.Published)
        {
            var recipients = await (
                from youth in _db.Users
                join cohort in _db.Cohorts on youth.CohortId equals cohort.Id
                where youth.Role == Roles.Youth
                    && youth.Status == UserStatus.Active
                    && cohort.TrackId == trackId
                select new { youth.Id, CohortId = (Guid?)cohort.Id })
                .ToListAsync(cancellationToken);
            var recipientIds = recipients.Select(item => item.Id).ToList();
            var existing = await _db.AssignedWork
                .Where(item => item.ItemType == "Project"
                    && item.ItemId == assignment.Id
                    && recipientIds.Contains(item.YouthUserId))
                .Select(item => item.YouthUserId)
                .ToListAsync(cancellationToken);
            foreach (var recipient in recipients.Where(item => !existing.Contains(item.Id)))
            {
                _db.AssignedWork.Add(new AssignedWork
                {
                    ItemType = "Project",
                    ItemId = assignment.Id,
                    YouthUserId = recipient.Id,
                    CohortId = recipient.CohortId,
                    DueDate = assignment.DueDate,
                    AssignedByUserId = _currentUser.UserId
                        ?? throw new ForbiddenException("Not signed in."),
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "AssignmentPublishStateChanged",
            _currentUser.UserId,
            "Assignment",
            assignment.Id,
            details: request.Status.ToString(),
            cancellationToken: cancellationToken);
    }
}

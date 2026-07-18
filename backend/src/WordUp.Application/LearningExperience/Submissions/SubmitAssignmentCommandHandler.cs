using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.Submissions;

public class SubmitAssignmentCommandHandler : IRequestHandler<SubmitAssignmentCommand, SubmissionResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public SubmitAssignmentCommandHandler(
        IAppDbContext db, IClock clock, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<SubmissionResult> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId
            ?? throw new ForbiddenException("No current user.");

        // The assignment must exist before a youth can submit against it.
        var assignmentExists = await _db.Assignments
            .AnyAsync(a => a.Id == request.AssignmentId, cancellationToken);
        if (!assignmentExists)
        {
            throw new NotFoundException("Assignment not found.");
        }

        var now = _clock.UtcNow;
        var submission = new Submission
        {
            YouthUserId = youthId,
            AssignmentId = request.AssignmentId,
            BodyText = request.BodyText,
            PhotoUrl = request.PhotoUrl,
            LinkUrl = request.LinkUrl,
            IsCodingProject = request.IsCodingProject,
            Status = SubmissionStatus.Submitted,
            SubmittedAt = now,
            CreatedAt = now,
        };

        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("SubmissionCreated", youthId, "Submission", submission.Id);

        return new SubmissionResult(submission.Id);
    }
}

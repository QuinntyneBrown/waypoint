using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.Submissions;

public class SubmissionStatusQueryHandler : IRequestHandler<SubmissionStatusQuery, SubmissionStatusView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SubmissionStatusQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SubmissionStatusView> Handle(SubmissionStatusQuery request, CancellationToken cancellationToken)
    {
        var submission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");

        // Owner-only: a youth may only view their own submission (L2-LEARN-007, no-IDOR).
        if (submission.YouthUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("You may only view your own submissions.");
        }

        return new SubmissionStatusView(
            submission.Id,
            submission.Status.ToString(),
            submission.MentorComment,
            submission.TotalAwarded,
            submission.SubmittedAt,
            submission.ReviewedAt);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Mentoring.MentorRoster;

public sealed class ReviewQueueQueryHandler
    : IRequestHandler<ReviewQueueQuery, IReadOnlyList<MentorReviewPreview>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public ReviewQueueQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<MentorReviewPreview>> Handle(
        ReviewQueueQuery request,
        CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId ?? throw new ForbiddenException();
        var assignedYouth = await _db.Users
            .Where(user => user.Role == Roles.Youth && user.MentorId == mentorId)
            .Select(user => new { user.Id, user.DisplayName })
            .ToListAsync(cancellationToken);
        var youthById = assignedYouth.ToDictionary(user => user.Id, user => user.DisplayName);
        var youthIds = youthById.Keys.ToList();

        var submissions = await _db.Submissions
            .Where(submission => youthIds.Contains(submission.YouthUserId)
                && (submission.Status == SubmissionStatus.Submitted
                    || submission.Status == SubmissionStatus.UnderReview))
            .Select(submission => new
            {
                submission.Id,
                submission.YouthUserId,
                submission.AssignmentId,
                submission.IsCodingProject,
                submission.SubmittedAt,
            })
            .ToListAsync(cancellationToken);
        var assignmentIds = submissions.Select(submission => submission.AssignmentId).Distinct().ToList();
        var assignments = await _db.Assignments
            .Where(assignment => assignmentIds.Contains(assignment.Id))
            .Select(assignment => new { assignment.Id, assignment.Title, assignment.DueDate })
            .ToDictionaryAsync(assignment => assignment.Id, cancellationToken);

        var reviews = submissions.Select(submission =>
        {
            assignments.TryGetValue(submission.AssignmentId, out var assignment);
            var dueDate = assignment?.DueDate;
            return new MentorReviewPreview(
                submission.Id,
                submission.YouthUserId,
                youthById[submission.YouthUserId],
                submission.IsCodingProject ? "Coding project" : "Assignment",
                assignment?.Title ?? "Assignment",
                submission.SubmittedAt,
                dueDate,
                dueDate != null && dueDate < _clock.UtcNow,
                $"/mentor/reviews/submissions/{submission.Id}");
        }).ToList();

        var attempts = await _db.QuizAttempts
            .Where(attempt => youthIds.Contains(attempt.YouthUserId)
                && attempt.IsSubmitted
                && attempt.ApprovedScore == null)
            .Select(attempt => new
            {
                attempt.Id,
                attempt.YouthUserId,
                attempt.QuizId,
                attempt.SubmittedAt,
                attempt.CreatedAt,
            })
            .ToListAsync(cancellationToken);
        var quizIds = attempts.Select(attempt => attempt.QuizId).Distinct().ToList();
        var quizzes = await _db.Quizzes
            .Where(quiz => quizIds.Contains(quiz.Id))
            .Select(quiz => new { quiz.Id, quiz.ModuleId, quiz.LessonId })
            .ToListAsync(cancellationToken);
        var moduleIds = quizzes
            .Where(quiz => quiz.ModuleId.HasValue)
            .Select(quiz => quiz.ModuleId!.Value)
            .Distinct()
            .ToList();
        var lessonIds = quizzes
            .Where(quiz => quiz.LessonId.HasValue)
            .Select(quiz => quiz.LessonId!.Value)
            .Distinct()
            .ToList();
        var moduleNames = await _db.Modules
            .Where(module => moduleIds.Contains(module.Id))
            .ToDictionaryAsync(module => module.Id, module => module.Name, cancellationToken);
        var lessonTitles = await _db.Lessons
            .Where(lesson => lessonIds.Contains(lesson.Id))
            .ToDictionaryAsync(lesson => lesson.Id, lesson => lesson.Title, cancellationToken);
        var quizTitles = quizzes.ToDictionary(
            quiz => quiz.Id,
            quiz => quiz.ModuleId is Guid moduleId && moduleNames.TryGetValue(moduleId, out var moduleName)
                ? $"{moduleName} quiz"
                : quiz.LessonId is Guid lessonId && lessonTitles.TryGetValue(lessonId, out var lessonTitle)
                    ? $"{lessonTitle} quiz"
                    : "Quiz assessment");
        reviews.AddRange(attempts.Select(attempt => new MentorReviewPreview(
            attempt.Id,
            attempt.YouthUserId,
            youthById[attempt.YouthUserId],
            "Quiz",
            quizTitles.GetValueOrDefault(attempt.QuizId, "Quiz assessment"),
            attempt.SubmittedAt ?? attempt.CreatedAt,
            null,
            false,
            $"/mentor/reviews/quizzes/{attempt.Id}")));

        var take = Math.Clamp(request.Take, 1, 100);
        return reviews
            .OrderByDescending(review => review.Overdue)
            .ThenBy(review => review.DueDate ?? DateTime.MaxValue)
            .ThenBy(review => review.SubmittedAt)
            .Take(take)
            .ToList();
    }
}

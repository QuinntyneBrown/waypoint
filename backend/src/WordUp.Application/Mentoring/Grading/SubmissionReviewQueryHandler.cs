using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.ConsentAndSafeguarding;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.Grading;

public sealed class SubmissionReviewQueryHandler : IRequestHandler<SubmissionReviewQuery, SubmissionReviewView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmissionReviewQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<SubmissionReviewView> Handle(SubmissionReviewQuery request, CancellationToken cancellationToken)
    {
        var submission = await _db.Submissions
            .FirstOrDefaultAsync(item => item.Id == request.SubmissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        var youth = await _db.Users
            .FirstOrDefaultAsync(item => item.Id == submission.YouthUserId, cancellationToken)
            ?? throw new NotFoundException("Submission owner not found.");

        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var assignment = await _db.Assignments
            .FirstOrDefaultAsync(item => item.Id == submission.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("Assignment not found.");
        var scores = ParseScores(submission.RubricScoresJson);
        var rubric = await _db.RubricCriteria
            .Where(item => item.AssignmentId == assignment.Id)
            .OrderBy(item => item.Order)
            .Select(item => new SubmissionReviewCriterion(
                item.Id, item.Description, item.Points,
                scores.ContainsKey(item.Id) ? scores[item.Id] : null))
            .ToListAsync(cancellationToken);
        var totalPossible = rubric.Count == 0 ? assignment.TotalPoints : rubric.Sum(item => item.PossiblePoints);
        var photoUrl = submission.PhotoUrl;
        if (photoUrl is not null
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                submission.YouthUserId,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            photoUrl = null;
        }

        return new SubmissionReviewView(
            submission.Id,
            youth.Id,
            youth.DisplayName,
            assignment.Title,
            assignment.TaskDescription,
            submission.Status.ToString(),
            submission.IsCodingProject,
            submission.BodyText,
            photoUrl,
            submission.LinkUrl,
            submission.YouthNote,
            submission.SubmittedAt,
            submission.MentorComment,
            submission.TotalAwarded,
            totalPossible,
            submission.GuardianNotified,
            rubric);
    }

    private static IReadOnlyDictionary<Guid, int> ParseScores(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<Guid, int>();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<Guid, int>>(json)
                ?? new Dictionary<Guid, int>();
        }
        catch (JsonException)
        {
            return new Dictionary<Guid, int>();
        }
    }
}

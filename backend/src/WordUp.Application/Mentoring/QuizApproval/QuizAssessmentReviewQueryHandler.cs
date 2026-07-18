using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.LearningExperience.QuizTaking;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.QuizApproval;

public sealed class QuizAssessmentReviewQueryHandler
    : IRequestHandler<QuizAssessmentReviewQuery, QuizAssessmentReviewView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public QuizAssessmentReviewQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<QuizAssessmentReviewView> Handle(
        QuizAssessmentReviewQuery request,
        CancellationToken cancellationToken)
    {
        var attempt = await _db.QuizAttempts
            .FirstOrDefaultAsync(item => item.Id == request.AttemptId, cancellationToken)
            ?? throw new NotFoundException("Quiz attempt not found.");
        if (!attempt.IsSubmitted)
        {
            throw new ConflictException("This quiz attempt has not been submitted.");
        }

        var youth = await _db.Users
            .FirstOrDefaultAsync(item => item.Id == attempt.YouthUserId, cancellationToken)
            ?? throw new NotFoundException("Attempt owner not found.");
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var quiz = await _db.Quizzes
            .FirstOrDefaultAsync(item => item.Id == attempt.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");
        var quizTitle = "Quiz";
        if (quiz.ModuleId is Guid moduleId)
        {
            quizTitle = await _db.Modules
                .Where(item => item.Id == moduleId)
                .Select(item => item.Name + " quiz")
                .FirstOrDefaultAsync(cancellationToken)
                ?? quizTitle;
        }
        else if (quiz.LessonId is Guid lessonId)
        {
            quizTitle = await _db.Lessons
                .Where(item => item.Id == lessonId)
                .Select(item => item.Title + " quiz")
                .FirstOrDefaultAsync(cancellationToken)
                ?? quizTitle;
        }

        var answers = QuizAnswerScoring.ParseAnswers(attempt.AnswersJson);
        var questions = await _db.QuizQuestions
            .Where(item => item.QuizId == quiz.Id)
            .OrderBy(item => item.Order)
            .ToListAsync(cancellationToken);
        var questionViews = questions.Select(question =>
        {
            var selected = answers.TryGetValue(question.Id, out var selectedSet)
                ? selectedSet.OrderBy(index => index).ToList()
                : new List<int>();
            var correct = QuizAnswerScoring.ParseIntSet(question.CorrectAnswersJson)
                .OrderBy(index => index)
                .ToList();
            return new QuizAssessmentQuestionView(
                question.Id,
                question.Text,
                QuizAnswerScoring.ParseOptions(question.OptionsJson),
                selected,
                correct,
                selected.SequenceEqual(correct));
        }).ToList();
        string? approvedByName = null;
        if (attempt.ApprovedByUserId is Guid approverId)
        {
            approvedByName = await _db.Users
                .Where(item => item.Id == approverId)
                .Select(item => item.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new QuizAssessmentReviewView(
            attempt.Id,
            youth.Id,
            youth.DisplayName,
            quiz.Id,
            quizTitle,
            attempt.AttemptNumber,
            attempt.AutoScore,
            attempt.ApprovedScore,
            quiz.PassMark,
            attempt.AutoScore >= quiz.PassMark,
            attempt.ApprovedScore is int approvedScore ? approvedScore >= quiz.PassMark : null,
            attempt.ApprovedAt.HasValue ? "Approved" : "Pending",
            attempt.SubmittedAt ?? attempt.CreatedAt,
            attempt.ApprovedAt,
            approvedByName,
            questionViews);
    }
}

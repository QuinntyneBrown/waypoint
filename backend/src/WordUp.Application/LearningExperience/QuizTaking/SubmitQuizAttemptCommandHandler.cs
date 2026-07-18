using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.QuizTaking;

public class SubmitQuizAttemptCommandHandler : IRequestHandler<SubmitQuizAttemptCommand, QuizResultView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public SubmitQuizAttemptCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
    }

    public async Task<QuizResultView> Handle(SubmitQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (attempt.YouthUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This attempt does not belong to you.");
        }

        if (attempt.IsSubmitted)
        {
            throw new ConflictException("This attempt has already been submitted.");
        }

        var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == attempt.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        var questions = await _db.QuizQuestions
            .Where(q => q.QuizId == quiz.Id)
            .ToListAsync(cancellationToken);

        // Score server-side against the stored correct answers (never trust the client).
        var answers = QuizAnswerScoring.ParseAnswers(request.AnswersJson);
        var correct = 0;
        foreach (var question in questions)
        {
            var expected = QuizAnswerScoring.ParseIntSet(question.CorrectAnswersJson);
            if (expected.Count > 0 && answers.TryGetValue(question.Id, out var given) && given.SetEquals(expected))
            {
                correct++;
            }
        }

        var total = questions.Count;
        var autoScore = total == 0 ? 0 : (int)Math.Round(100.0 * correct / total, MidpointRounding.AwayFromZero);

        attempt.AnswersJson = request.AnswersJson;
        attempt.AutoScore = autoScore;
        attempt.Passed = autoScore >= quiz.PassMark;
        attempt.IsSubmitted = true;
        attempt.SubmittedAt = _clock.UtcNow;
        // ApprovedScore intentionally left null: a mentor must approve before the youth sees a score.

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("QuizAttemptSubmitted", _currentUser.UserId, "QuizAttempt", attempt.Id);

        // The youth does not see the score until approved (L2-LEARN-005 AC3).
        return new QuizResultView(null, null, "PendingApproval");
    }
}

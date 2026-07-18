using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.QuizTaking;

public class QuizResultQueryHandler : IRequestHandler<QuizResultQuery, QuizResultView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public QuizResultQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<QuizResultView> Handle(QuizResultQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (attempt.YouthUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This attempt does not belong to you.");
        }

        if (attempt.ApprovedScore is null)
        {
            // Withhold the score until a mentor approves (L2-LEARN-005 AC3).
            return new QuizResultView(null, null, "PendingApproval");
        }

        var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == attempt.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        var approved = attempt.ApprovedScore.Value;
        return new QuizResultView(approved, approved >= quiz.PassMark, "Approved");
    }
}

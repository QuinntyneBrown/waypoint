using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.QuizTaking;

public class SaveAnswersCommandHandler : IRequestHandler<SaveAnswersCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SaveAnswersCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(SaveAnswersCommand request, CancellationToken cancellationToken)
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

        attempt.AnswersJson = request.AnswersJson;
        var questionCount = await _db.QuizQuestions
            .CountAsync(question => question.QuizId == attempt.QuizId, cancellationToken);
        attempt.CurrentQuestionIndex = questionCount == 0
            ? 0
            : Math.Clamp(request.CurrentQuestionIndex, 0, questionCount - 1);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

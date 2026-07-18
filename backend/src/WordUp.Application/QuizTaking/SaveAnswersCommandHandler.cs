using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.QuizTaking;

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
        await _db.SaveChangesAsync(cancellationToken);
    }
}

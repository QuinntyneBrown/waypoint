using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed class UpdateQuizCommandHandler : IRequestHandler<UpdateQuizCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public UpdateQuizCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _db.Quizzes.FirstOrDefaultAsync(
            item => item.Id == request.QuizId,
            cancellationToken) ?? throw new NotFoundException("Quiz not found.");
        if (quiz.ModuleId != request.ModuleId || quiz.LessonId != request.LessonId)
        {
            throw new ConflictException("A quiz cannot be moved while editing its settings.");
        }

        quiz.PassMark = request.PassMark;
        quiz.AttemptsAllowed = request.AttemptsAllowed;
        quiz.FeedbackMode = request.FeedbackMode;
        quiz.Shuffle = request.Shuffle;
        quiz.RequiredToCompleteModule = request.RequiredToCompleteModule;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "QuizUpdated",
            _currentUser.UserId,
            "Quiz",
            quiz.Id,
            cancellationToken: cancellationToken);
    }
}

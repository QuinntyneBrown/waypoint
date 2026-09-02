using MediatR;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public class CreateQuizCommandHandler : IRequestHandler<CreateQuizCommand, QuizResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public CreateQuizCommandHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<QuizResult> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = new Quiz
        {
            ModuleId = request.ModuleId,
            LessonId = request.LessonId,
            PassMark = request.PassMark,
            AttemptsAllowed = request.AttemptsAllowed,
            FeedbackMode = request.FeedbackMode,
            Shuffle = request.Shuffle,
            RequiredToCompleteModule = request.RequiredToCompleteModule,
            CreatedAt = _clock.UtcNow,
        };

        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync(cancellationToken);

        return new QuizResult(quiz.Id);
    }
}

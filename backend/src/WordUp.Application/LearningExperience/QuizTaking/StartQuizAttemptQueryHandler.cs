using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;
using WordUp.Application.LearningExperience.YouthCurriculum;

namespace WordUp.Application.LearningExperience.QuizTaking;

public class StartQuizAttemptQueryHandler : IRequestHandler<StartQuizAttemptQuery, QuizAttemptView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public StartQuizAttemptQueryHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<QuizAttemptView> Handle(StartQuizAttemptQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");

        var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        if (quiz.ModuleId is { } moduleId)
        {
            var module = await _db.Modules.FirstOrDefaultAsync(item => item.Id == moduleId, cancellationToken)
                ?? throw new NotFoundException("Module not found.");
            var enrolledTrackIds = await CurriculumProgressCalculator.ResolveYouthTrackIdsAsync(
                _db,
                youthId,
                cancellationToken);
            if (module.Status != PublishStatus.Published || !enrolledTrackIds.Contains(module.TrackId))
            {
                throw new ForbiddenException("This quiz is locked.");
            }

            var moduleStatus = (await CurriculumProgressCalculator.BuildModuleStatusesAsync(
                    _db,
                    module.TrackId,
                    youthId,
                    cancellationToken))
                .FirstOrDefault(item => item.ModuleId == module.Id);
            var quizItem = moduleStatus?.Items.FirstOrDefault(item =>
                item.Type == "quiz" && item.ItemId == quiz.Id);
            if (moduleStatus is null || moduleStatus.Locked || quizItem is null || quizItem.Locked)
            {
                throw new ForbiddenException("This quiz is locked.");
            }
        }

        var attempts = await _db.QuizAttempts
            .Where(a => a.QuizId == quiz.Id && a.YouthUserId == youthId)
            .ToListAsync(cancellationToken);

        var submittedCount = attempts.Count(a => a.IsSubmitted);
        var open = attempts.FirstOrDefault(a => !a.IsSubmitted);

        QuizAttempt attempt;
        if (open is not null)
        {
            // Resume the in-progress attempt (L2-LEARN-004 AC2).
            attempt = open;
        }
        else
        {
            if (submittedCount >= quiz.AttemptsAllowed)
            {
                throw new ConflictException("No attempts remaining.");
            }

            attempt = new QuizAttempt
            {
                YouthUserId = youthId,
                QuizId = quiz.Id,
                AttemptNumber = submittedCount + 1,
            };
            _db.QuizAttempts.Add(attempt);
            await _db.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync("QuizAttemptStarted", youthId, "QuizAttempt", attempt.Id);
        }

        var questions = await _db.QuizQuestions
            .Where(q => q.QuizId == quiz.Id)
            .OrderBy(q => q.Order)
            .ToListAsync(cancellationToken);

        var questionDtos = questions
            .Select(q => new QuestionDto(q.Id, q.Text, q.Type.ToString(), QuizAnswerScoring.ParseOptions(q.OptionsJson)))
            .ToList();

        return new QuizAttemptView(
            attempt.Id,
            attempt.AttemptNumber,
            questions.Count,
            quiz.AttemptsAllowed,
            quiz.PassMark,
            questionDtos);
    }
}

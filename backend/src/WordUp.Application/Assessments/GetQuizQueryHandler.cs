using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.Assessments;

public class GetQuizQueryHandler : IRequestHandler<GetQuizQuery, QuizDetail>
{
    private readonly IAppDbContext _db;

    public GetQuizQueryHandler(IAppDbContext db) => _db = db;

    public async Task<QuizDetail> Handle(GetQuizQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        var questions = await _db.QuizQuestions
            .Where(q => q.QuizId == quiz.Id)
            .OrderBy(q => q.Order)
            .ToListAsync(cancellationToken);

        var questionDetails = questions
            .Select(q => new QuizQuestionDetail(
                q.Id,
                q.Text,
                q.Type,
                q.Order,
                Deserialize<string>(q.OptionsJson),
                Deserialize<int>(q.CorrectAnswersJson)))
            .ToList();

        return new QuizDetail(
            quiz.Id,
            quiz.ModuleId,
            quiz.LessonId,
            quiz.PassMark,
            quiz.AttemptsAllowed,
            quiz.FeedbackMode,
            quiz.Shuffle,
            quiz.RequiredToCompleteModule,
            questionDetails.Count,
            questionDetails);
    }

    private static IReadOnlyList<T> Deserialize<T>(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? Array.Empty<T>()
            : JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
}

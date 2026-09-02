using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public class AddQuizQuestionCommandHandler : IRequestHandler<AddQuizQuestionCommand, QuestionResult>
{
    private readonly IAppDbContext _db;

    public AddQuizQuestionCommandHandler(IAppDbContext db) => _db = db;

    public async Task<QuestionResult> Handle(AddQuizQuestionCommand request, CancellationToken cancellationToken)
    {
        var quizExists = await _db.Quizzes.AnyAsync(q => q.Id == request.QuizId, cancellationToken);
        if (!quizExists)
        {
            throw new NotFoundException("Quiz not found.");
        }

        // Order is the next position within this quiz.
        var nextOrder = await _db.QuizQuestions.CountAsync(q => q.QuizId == request.QuizId, cancellationToken);

        var question = new QuizQuestion
        {
            QuizId = request.QuizId,
            Text = request.Text.Trim(),
            Type = request.Type,
            Order = nextOrder,
            OptionsJson = JsonSerializer.Serialize(
                request.Options?.Select(option => option.Trim()).ToList() ?? []),
            CorrectAnswersJson = request.Type == QuestionType.ShortAnswer
                ? JsonSerializer.Serialize(request.CorrectAnswers
                    .Select(answer => answer.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                : JsonSerializer.Serialize(request.CorrectIndexes.Distinct()),
        };

        _db.QuizQuestions.Add(question);
        await _db.SaveChangesAsync(cancellationToken);

        return new QuestionResult(question.Id);
    }
}

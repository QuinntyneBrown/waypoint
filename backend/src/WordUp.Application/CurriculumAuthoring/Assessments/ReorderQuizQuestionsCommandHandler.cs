using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Assessments;

public sealed class ReorderQuizQuestionsCommandHandler
    : IRequestHandler<ReorderQuizQuestionsCommand>
{
    private readonly IAppDbContext _db;

    public ReorderQuizQuestionsCommandHandler(IAppDbContext db) => _db = db;

    public async Task Handle(
        ReorderQuizQuestionsCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _db.Quizzes.AnyAsync(item => item.Id == request.QuizId, cancellationToken))
        {
            throw new NotFoundException("Quiz not found.");
        }
        var questions = await _db.QuizQuestions
            .Where(item => item.QuizId == request.QuizId)
            .ToListAsync(cancellationToken);
        var requestedIds = request.OrderedQuestionIds?.ToList() ?? [];
        if (requestedIds.Count != questions.Count
            || requestedIds.Distinct().Count() != requestedIds.Count
            || requestedIds.Except(questions.Select(item => item.Id)).Any())
        {
            throw new ConflictException("The question order must contain every question exactly once.");
        }

        var byId = questions.ToDictionary(item => item.Id);
        for (var index = 0; index < requestedIds.Count; index++)
        {
            byId[requestedIds[index]].Order = index;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }
}

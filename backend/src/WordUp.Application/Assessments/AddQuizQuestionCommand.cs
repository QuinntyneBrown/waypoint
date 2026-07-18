using MediatR;
using WordUp.Domain;

namespace WordUp.Application.Assessments;

public record AddQuizQuestionCommand(
    Guid QuizId,
    string Text,
    QuestionType Type,
    IReadOnlyList<string> Options,
    IReadOnlyList<int> CorrectIndexes) : IRequest<QuestionResult>;

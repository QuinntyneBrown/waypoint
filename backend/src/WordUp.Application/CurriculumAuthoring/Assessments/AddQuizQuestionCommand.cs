using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Assessments;

public record AddQuizQuestionCommand(
    Guid QuizId,
    string Text,
    QuestionType Type,
    IReadOnlyList<string> Options,
    IReadOnlyList<int> CorrectIndexes) : IRequest<QuestionResult>;

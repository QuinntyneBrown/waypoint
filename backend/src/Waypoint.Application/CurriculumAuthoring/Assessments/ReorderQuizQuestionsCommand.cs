using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed record ReorderQuizQuestionsCommand(
    Guid QuizId,
    IReadOnlyList<Guid> OrderedQuestionIds) : IRequest;

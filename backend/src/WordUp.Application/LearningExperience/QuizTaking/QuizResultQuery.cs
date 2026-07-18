using MediatR;

namespace WordUp.Application.LearningExperience.QuizTaking;

/// <summary>Returns a youth's own attempt result; the score appears only once approved (L2-LEARN-005 AC1/AC3).</summary>
public record QuizResultQuery(Guid AttemptId) : IRequest<QuizResultView>;

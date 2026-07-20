using MediatR;

namespace WordUp.Application.LearningExperience.QuizTaking;

/// <summary>Submits an attempt for scoring. Immutable once submitted (L2-LEARN-004 AC5). The auto score
/// is recorded but withheld from the youth until a mentor approves it (L2-MENTOR-004).</summary>
public record SubmitQuizAttemptCommand(Guid AttemptId, string AnswersJson)
    : IRequest<QuizResultView>, IAtomicOutboxRequest;

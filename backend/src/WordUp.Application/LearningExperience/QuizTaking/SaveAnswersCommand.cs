using MediatR;

namespace WordUp.Application.LearningExperience.QuizTaking;

/// <summary>Autosaves the youth's in-progress answers for an unsubmitted attempt (L2-LEARN-004 AC2).</summary>
public record SaveAnswersCommand(Guid AttemptId, string AnswersJson) : IRequest;

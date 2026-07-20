using MediatR;

namespace WordUp.Application.Mentoring.QuizApproval;

/// <summary>A mentor (or administrator) approves a youth's quiz attempt, optionally overriding the
/// auto-scored result (L2-MENTOR-004, L2-LEARN-005 AC2/AC3). The auto score is retained for audit.</summary>
public record ApproveQuizAttemptCommand(Guid AttemptId, int? OverrideScore)
    : IRequest, IAtomicOutboxRequest;

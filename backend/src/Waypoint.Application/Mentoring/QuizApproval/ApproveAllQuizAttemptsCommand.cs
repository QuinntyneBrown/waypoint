using MediatR;

namespace Waypoint.Application.Mentoring.QuizApproval;

/// <summary>A mentor approves every pending (submitted, not-yet-approved) quiz attempt for their
/// assigned youth in one action, returning the number approved (L2-MENTOR-004 AC3).</summary>
public record ApproveAllQuizAttemptsCommand() : IRequest<int>, IAtomicOutboxRequest;

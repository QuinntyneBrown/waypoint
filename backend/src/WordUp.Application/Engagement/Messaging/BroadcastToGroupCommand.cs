using MediatR;

namespace WordUp.Application.Engagement.Messaging;

/// <summary>A mentor broadcasts a message to their whole roster (L2-ENGAGE-007). Fans out into one
/// individual personal message per assigned youth. The body is stored HTML-encoded (L2-NFR-001).</summary>
public record BroadcastToGroupCommand(Guid CohortId, string Body) : IRequest<BroadcastResult>;

public record BroadcastResult(int Sent);

using MediatR;

namespace WordUp.Application.Messaging;

/// <summary>Sends a 1:1 direct message (L2-ENGAGE-007). Only permitted within the mentoring relationship
/// (youth ↔ their mentor) or by a Program Administrator. The body is stored HTML-encoded (L2-NFR-001).</summary>
public record SendMessageCommand(Guid ToUserId, string Body) : IRequest<MessageResult>;

public record MessageResult(Guid Id);

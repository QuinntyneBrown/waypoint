using MediatR;

namespace WordUp.Application.Messaging;

/// <summary>The message thread between the current user and another user (L2-ENGAGE-007). Only a
/// participant of the mentoring relationship (or an admin) may read it; otherwise 403.</summary>
public record MessageThreadQuery(Guid OtherUserId) : IRequest<IReadOnlyList<MessageView>>;

public record MessageView(Guid Id, Guid SenderId, Guid RecipientId, string Body, DateTime SentAt, DateTime? ReadAt);

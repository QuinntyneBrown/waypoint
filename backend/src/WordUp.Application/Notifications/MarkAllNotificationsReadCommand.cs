using MediatR;

namespace WordUp.Application.Notifications;

/// <summary>Marks all of the current user's unread notifications as read (L2-ENGAGE-008).</summary>
public record MarkAllNotificationsReadCommand() : IRequest;

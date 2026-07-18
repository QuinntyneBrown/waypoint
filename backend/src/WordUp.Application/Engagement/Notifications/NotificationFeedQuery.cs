using MediatR;

namespace WordUp.Application.Engagement.Notifications;

/// <summary>The current user's in-app notification feed, newest first (L2-ENGAGE-008).</summary>
public record NotificationFeedQuery() : IRequest<IReadOnlyList<NotificationView>>;

public record NotificationView(
    Guid Id,
    string EventKey,
    string Title,
    string Body,
    string? LinkPath,
    bool Read,
    DateTime CreatedAt);

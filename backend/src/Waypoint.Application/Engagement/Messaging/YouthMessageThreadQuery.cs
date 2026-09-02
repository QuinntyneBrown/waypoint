using MediatR;

namespace Waypoint.Application.Engagement.Messaging;

public sealed record YouthMessageThreadQuery : IRequest<YouthMessageThreadView>;

public sealed record YouthMessageThreadView(
    Guid? MentorId,
    string? MentorName,
    int UnreadCount,
    IReadOnlyList<MessageView> Messages);

using MediatR;

namespace WordUp.Application.Engagement.Messaging;

/// <summary>The current mentor's active youth conversations and cohort broadcast choices.</summary>
public sealed record MentorMessageInboxQuery : IRequest<MentorMessageInboxView>;

public sealed record MentorMessageInboxView(
    IReadOnlyList<MentorConversationView> Conversations,
    IReadOnlyList<MentorBroadcastGroupView> Groups);

public sealed record MentorConversationView(
    Guid YouthId,
    string YouthName,
    Guid? CohortId,
    string? CohortName,
    int UnreadCount,
    string? LastMessagePreview,
    DateTime? LastMessageAt);

public sealed record MentorBroadcastGroupView(Guid CohortId, string CohortName, int RecipientCount);

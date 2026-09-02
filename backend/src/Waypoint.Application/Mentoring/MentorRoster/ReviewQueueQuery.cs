using MediatR;

namespace Waypoint.Application.Mentoring.MentorRoster;

/// <summary>Pending assignment and quiz reviews for the current mentor's youth (L2-MENTOR-001 AC3/AC4).</summary>
public sealed record ReviewQueueQuery(int Take = 20) : IRequest<IReadOnlyList<MentorReviewPreview>>;

public sealed record MentorReviewPreview(
    Guid ReviewId,
    Guid YouthId,
    string YouthName,
    string ItemType,
    string ItemTitle,
    DateTime SubmittedAt,
    DateTime? DueDate,
    bool Overdue,
    string ReviewPath);

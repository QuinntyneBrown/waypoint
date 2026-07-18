using MediatR;

namespace WordUp.Application.Mentoring.MentorRoster;

/// <summary>Dashboard summary for the current mentor, computed over their assigned youth (L2-MENTOR-001 AC1).</summary>
public record MentorDashboardQuery() : IRequest<MentorDashboard>;

public record MentorDashboard(
    int YouthCount,
    int PendingReviews,
    int OverdueReviews,
    double AverageProgress,
    int AtRiskCount);

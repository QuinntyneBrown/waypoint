namespace WordUp.Application.Shared.Abstractions;

/// <summary>Configuration-backed thresholds for automated at-risk evaluation (L2-MENTOR-007 AC1).</summary>
public interface IAtRiskThresholds
{
    int InactiveDays { get; }
    int MissedSessions { get; }
    double ProgressBelowCohortPercentage { get; }
}

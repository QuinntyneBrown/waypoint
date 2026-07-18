namespace WordUp.Application.LearningExperience.QuizTaking;

/// <summary>The result a youth may see. Score/Passed stay null until a mentor approves the attempt
/// (L2-LEARN-005 AC3, L2-MENTOR-004).</summary>
public record QuizResultView(int? Score, bool? Passed, string Status);

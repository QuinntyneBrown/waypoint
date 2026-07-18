using MediatR;

namespace WordUp.Application.Flags;

/// <summary>A mentor resolves (clears) an active at-risk flag (L2-MENTOR-007 AC3/AC4).</summary>
public record ResolveFlagCommand(Guid FlagId, string ResolutionReason) : IRequest;

public static class FlagResolutionReasons
{
    public const string Reconnected = "Reconnected";
    public const string HandedToDirector = "HandedToDirector";
    public const string LeftProgram = "LeftProgram";
    public const string FlaggedInError = "FlaggedInError";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string> { Reconnected, HandedToDirector, LeftProgram, FlaggedInError };

    public static bool IsValid(string reason) => All.Contains(reason);
}

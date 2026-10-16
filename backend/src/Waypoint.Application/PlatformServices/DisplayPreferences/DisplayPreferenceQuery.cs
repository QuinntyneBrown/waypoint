using MediatR;

namespace Waypoint.Application.PlatformServices.DisplayPreferences;

/// <summary>Returns the signed-in user's display &amp; accessibility preferences, or platform defaults
/// when none have been saved (L2-PLATFORM-004).</summary>
public record DisplayPreferenceQuery : IRequest<DisplayPreferenceView>;

public record DisplayPreferenceView(
    int TextScalePercent,
    bool HighContrast,
    bool ReduceMotion,
    bool UnderlineLinks,
    string? ReadingFont,
    string Theme);

using MediatR;

namespace Waypoint.Application.PlatformServices.DisplayPreferences;

/// <summary>Upserts the signed-in user's display &amp; accessibility preferences (L2-PLATFORM-004).
/// One row per user.</summary>
public record SaveDisplayPreferenceCommand(
    int TextScalePercent,
    bool HighContrast,
    bool ReduceMotion,
    bool UnderlineLinks,
    string? ReadingFont) : IRequest<DisplayPreferenceView>;

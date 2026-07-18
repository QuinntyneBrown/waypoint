namespace WordUp.Api.Contracts.PlatformServices;

public record SaveDisplayPreferenceRequest(
    int TextScalePercent,
    bool HighContrast,
    bool ReduceMotion,
    bool UnderlineLinks,
    string? ReadingFont);

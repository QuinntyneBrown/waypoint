using MediatR;

namespace WordUp.Application.NotificationPreferences;

/// <summary>Upserts one per-event notification preference for the signed-in user (L2-PLATFORM-003).
/// One row per (user, event); saving again for the same event updates it in place.</summary>
public record SaveNotificationPreferenceCommand(
    string EventKey,
    bool InApp,
    bool Email) : IRequest;

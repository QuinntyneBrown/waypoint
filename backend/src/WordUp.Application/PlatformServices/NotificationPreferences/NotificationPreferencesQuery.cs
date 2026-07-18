using MediatR;

namespace WordUp.Application.PlatformServices.NotificationPreferences;

/// <summary>Returns the signed-in user's saved per-event notification preferences (L2-PLATFORM-003).</summary>
public record NotificationPreferencesQuery : IRequest<IReadOnlyList<NotificationPreferenceView>>;

public record NotificationPreferenceView(string EventKey, bool InApp, bool Email);

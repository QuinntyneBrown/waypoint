namespace Waypoint.Api.Contracts.PlatformServices;

public record SaveNotificationPreferenceRequest(string EventKey, bool InApp, bool Email);

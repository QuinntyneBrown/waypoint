namespace Waypoint.Api.Contracts.PlatformServices;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);

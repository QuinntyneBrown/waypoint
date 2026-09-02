using MediatR;

namespace Waypoint.Application.PlatformServices.Profile;

/// <summary>Changes the signed-in user's password (L2-PLATFORM-002). Requires the current password
/// and invalidates other active sessions on success.</summary>
public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest;

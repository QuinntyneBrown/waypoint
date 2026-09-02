using MediatR;

namespace Waypoint.Application.Authentication.PasswordReset;

public record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword) : IRequest;

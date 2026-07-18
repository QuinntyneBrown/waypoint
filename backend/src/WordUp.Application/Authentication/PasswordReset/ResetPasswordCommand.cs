using MediatR;

namespace WordUp.Application.Authentication.PasswordReset;

public record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword) : IRequest;

using MediatR;

namespace WordUp.Application.PasswordReset;

public record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword) : IRequest;

using MediatR;

namespace WordUp.Application.PasswordReset;

public record ForgotPasswordCommand(string Email) : IRequest;

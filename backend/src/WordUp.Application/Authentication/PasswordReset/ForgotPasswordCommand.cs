using MediatR;

namespace WordUp.Application.Authentication.PasswordReset;

public record ForgotPasswordCommand(string Email) : IRequest, IAtomicOutboxRequest;

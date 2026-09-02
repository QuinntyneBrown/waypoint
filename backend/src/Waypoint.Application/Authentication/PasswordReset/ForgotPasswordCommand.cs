using MediatR;

namespace Waypoint.Application.Authentication.PasswordReset;

public record ForgotPasswordCommand(string Email) : IRequest, IAtomicOutboxRequest;

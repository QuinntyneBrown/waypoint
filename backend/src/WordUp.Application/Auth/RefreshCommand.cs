using MediatR;

namespace WordUp.Application.Auth;

public record RefreshCommand(string RefreshToken) : IRequest<AuthResult>;

using MediatR;

namespace WordUp.Application.Authentication.Auth;

public record RefreshCommand(string RefreshToken) : IRequest<AuthResult>;

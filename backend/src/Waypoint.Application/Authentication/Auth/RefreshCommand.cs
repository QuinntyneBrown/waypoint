using MediatR;

namespace Waypoint.Application.Authentication.Auth;

public record RefreshCommand(string RefreshToken) : IRequest<AuthResult>;

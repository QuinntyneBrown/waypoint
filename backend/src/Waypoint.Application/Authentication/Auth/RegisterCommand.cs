using MediatR;

namespace Waypoint.Application.Authentication.Auth;

public record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<AuthResult>;

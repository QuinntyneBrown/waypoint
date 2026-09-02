using MediatR;

namespace Waypoint.Application.Authentication.Auth;

public record SignInCommand(string Email, string Password) : IRequest<AuthResult>;

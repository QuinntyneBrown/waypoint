using MediatR;

namespace WordUp.Application.Authentication.Auth;

public record SignInCommand(string Email, string Password) : IRequest<AuthResult>;

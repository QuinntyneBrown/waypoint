using MediatR;

namespace WordUp.Application.Auth;

public record SignInCommand(string Email, string Password) : IRequest<AuthResult>;

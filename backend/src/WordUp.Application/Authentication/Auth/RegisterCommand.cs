using MediatR;

namespace WordUp.Application.Authentication.Auth;

public record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<AuthResult>;

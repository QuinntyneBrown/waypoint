using MediatR;

namespace WordUp.Application.Auth;

public record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<AuthResult>;

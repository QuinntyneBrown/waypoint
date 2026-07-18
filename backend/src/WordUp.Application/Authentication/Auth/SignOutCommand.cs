using MediatR;

namespace WordUp.Application.Authentication.Auth;

/// <summary>Signs the current user out of a shared device by revoking their refresh tokens (L2-AUTH-007).</summary>
public record SignOutCommand : IRequest;

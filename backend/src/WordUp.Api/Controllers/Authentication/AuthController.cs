using WordUp.Api.Contracts.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Authentication.Auth;

namespace WordUp.Api.Controllers.Authentication;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterCommand(request.Email, request.Password, request.DisplayName), cancellationToken);
        return Ok(result);
    }

    [HttpPost("sign-in")]
    public async Task<ActionResult<AuthResult>> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SignInCommand(request.Email, request.Password), cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshCommand(request.RefreshToken), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("sign-out")]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
    {
        await _mediator.Send(new SignOutCommand(), cancellationToken);
        return NoContent();
    }
}

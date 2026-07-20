using WordUp.Api.Contracts.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Authentication.PasswordReset;

namespace WordUp.Api.Controllers.Authentication;

[ApiController]
[Route("api/auth/password-reset")]
public class PasswordResetController : ControllerBase
{
    private readonly IMediator _mediator;

    public PasswordResetController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpPost("request")]
    public async Task<IActionResult> RequestReset([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword, request.ConfirmPassword), cancellationToken);
        return NoContent();
    }
}

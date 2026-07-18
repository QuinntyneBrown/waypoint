using WordUp.Api.Contracts.PlatformServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.PlatformServices.Profile;

namespace WordUp.Api.Controllers.PlatformServices;

[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator) => _mediator = mediator;

    [HttpPut("api/me/profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateProfileCommand(request.DisplayName, request.Email, request.Pronouns, request.PhotoUrl),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("api/me/change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ChangePasswordCommand(request.CurrentPassword, request.NewPassword, request.ConfirmPassword),
            cancellationToken);
        return NoContent();
    }
}

using WordUp.Api.Contracts.PlatformServices;
using WordUp.Application.PlatformServices.Files;
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

    [HttpGet("api/me/profile")]
    public async Task<ActionResult<MyProfileView>> Mine(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetMyProfileQuery(), cancellationToken));

    [HttpPut("api/me/profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateProfileCommand(request.DisplayName, request.Email, request.Pronouns),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("api/me/profile/photo")]
    [RequestSizeLimit(SafeImageUploadPolicy.MaxBytes + 65536)]
    public async Task<ActionResult<ProfilePhotoResult>> UploadPhoto(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        return Ok(await _mediator.Send(
            new StoreProfilePhotoCommand(
                file.FileName,
                file.ContentType.ToLowerInvariant(),
                stream.ToArray()),
            cancellationToken));
    }

    [HttpGet("api/profile-photos/{id:guid}")]
    public async Task<IActionResult> Photo(Guid id, CancellationToken cancellationToken)
    {
        var photo = await _mediator.Send(new GetProfilePhotoQuery(id), cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(photo.Content, photo.ContentType);
    }

    [HttpDelete("api/me/profile/photo")]
    public async Task<IActionResult> RemovePhoto(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveProfilePhotoCommand(), cancellationToken);
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

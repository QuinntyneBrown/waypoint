using FluentValidation;
using FluentValidation.Results;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

/// <summary>
/// A validation failure that carries its message to the client. A bare
/// <c>ValidationException(string)</c> has no field errors, so the message would be lost.
/// </summary>
internal static class VideoUploadRejection
{
    public static ValidationException Because(string message) =>
        new([new ValidationFailure("file", message)]);
}

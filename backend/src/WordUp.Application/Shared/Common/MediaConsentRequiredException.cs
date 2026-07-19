namespace WordUp.Application.Shared.Common;

public sealed class MediaConsentRequiredException : AppException
{
    public MediaConsentRequiredException()
        : base(403, "Guardian consent for individual photos or video is required.")
    {
    }
}

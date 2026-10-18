using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.LearningExperience.YouthCurriculum;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;
using Waypoint.Domain.CurriculumAuthoring;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

/// <summary>
/// Asks for a signed playback address. <paramref name="LessonId"/> is supplied by the youth route and
/// must match the media; the staff preview route leaves it null.
/// </summary>
public sealed record LessonVideoPlaybackQuery(Guid MediaId, Guid? LessonId)
    : IRequest<VideoPlaybackResult>;

public sealed record VideoPlaybackResult(
    string Url, DateTime ExpiresAt, double DurationSeconds, string PosterUrl);

public sealed class LessonVideoPlaybackQueryHandler
    : IRequestHandler<LessonVideoPlaybackQuery, VideoPlaybackResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPlaybackUrlSigner _signer;

    public LessonVideoPlaybackQueryHandler(
        IAppDbContext db, ICurrentUser currentUser, IPlaybackUrlSigner signer)
    {
        _db = db;
        _currentUser = currentUser;
        _signer = signer;
    }

    public async Task<VideoPlaybackResult> Handle(
        LessonVideoPlaybackQuery request, CancellationToken cancellationToken)
    {
        var media = await _db.LessonMedia.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == request.MediaId && item.MediaType == "video"
                && item.VideoStatus != VideoStatus.None,
            cancellationToken);
        if (media is null || (request.LessonId is { } lessonId && lessonId != media.LessonId))
        {
            throw new NotFoundException("Lesson media not found.");
        }

        if (_currentUser.Role == Roles.Youth)
        {
            var youthId = _currentUser.UserId ?? throw new ForbiddenException();
            await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
                _db, media.LessonId, youthId, cancellationToken);
        }
        else if (_currentUser.Role is not (Roles.ProgramAdministrator or Roles.Mentor))
        {
            throw new ForbiddenException("This media is not available to your role.");
        }

        if (media.VideoStatus != VideoStatus.Ready)
        {
            throw new ConflictException("The video is not ready to play yet.");
        }

        var signed = _signer.Sign(media.Id);
        var baseUrl = $"/api/media/{signed.Token}/{media.Id:D}";
        return new VideoPlaybackResult(
            $"{baseUrl}/master.m3u8", signed.ExpiresAt, media.DurationSeconds ?? 0, $"{baseUrl}/poster.jpg");
    }
}

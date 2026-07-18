using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.LearningExperience.Submissions;
using WordUp.Application.LearningExperience.YouthCurriculum;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.CodingLessons;

public sealed record SubmitCodingProjectCommand(
    Guid LessonId,
    string Code,
    string? MentorNote,
    bool Attest) : IRequest<SubmissionResult>;

public sealed class SubmitCodingProjectCommandHandler : IRequestHandler<SubmitCodingProjectCommand, SubmissionResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public SubmitCodingProjectCommandHandler(IAppDbContext db, ICurrentUser currentUser, IMediator mediator)
    {
        _db = db;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<SubmissionResult> Handle(SubmitCodingProjectCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var access = await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
            _db, request.LessonId, youthId, cancellationToken);
        if (!access.Lesson.IsCodingLesson) throw new NotFoundException("Coding lesson not found.");
        var assignmentId = await _db.Assignments
            .Where(item => item.LessonId == access.Lesson.Id)
            .Select(item => (Guid?)item.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Coding project not found.");

        return await _mediator.Send(
            new SubmitAssignmentCommand(
                assignmentId,
                request.Code,
                null,
                null,
                null,
                true,
                request.MentorNote,
                request.Attest),
            cancellationToken);
    }
}

using MediatR;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.Assessments;

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, AssignmentResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public CreateAssignmentCommandHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<AssignmentResult> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var totalPoints = request.Rubric?.Sum(r => r.Points) ?? 0;

        var assignment = new Assignment
        {
            ModuleId = request.ModuleId,
            LessonId = request.LessonId,
            Title = request.Title.Trim(),
            TaskDescription = request.TaskDescription,
            SubmissionTypes = string.Join(",", request.SubmissionTypes),
            DueDate = request.DueDate,
            GracePeriodDays = request.GracePeriodDays,
            AllowLate = request.AllowLate,
            RequireMentorApproval = request.RequireMentorApproval,
            TotalPoints = totalPoints,
            CreatedAt = _clock.UtcNow,
        };

        _db.Assignments.Add(assignment);

        if (request.Rubric is not null)
        {
            var order = 0;
            foreach (var criterion in request.Rubric)
            {
                _db.RubricCriteria.Add(new RubricCriterion
                {
                    AssignmentId = assignment.Id,
                    Description = criterion.Description,
                    Points = criterion.Points,
                    Order = order++,
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new AssignmentResult(assignment.Id, assignment.TotalPoints);
    }
}

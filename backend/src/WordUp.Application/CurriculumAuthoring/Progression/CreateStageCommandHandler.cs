using MediatR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public class CreateStageCommandHandler : IRequestHandler<CreateStageCommand, StageResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CreateStageCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<StageResult> Handle(CreateStageCommand request, CancellationToken cancellationToken)
    {
        var stage = new ProgressionStage
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Order = request.Order,
            CreatedAt = _clock.UtcNow,
        };

        _db.ProgressionStages.Add(stage);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ProgressionStageCreated",
            _currentUser.UserId,
            "ProgressionStage",
            stage.Id,
            cancellationToken: cancellationToken);

        return new StageResult(stage.Id);
    }
}

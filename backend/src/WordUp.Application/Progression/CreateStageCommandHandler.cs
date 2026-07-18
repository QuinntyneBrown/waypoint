using MediatR;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.Progression;

public class CreateStageCommandHandler : IRequestHandler<CreateStageCommand, StageResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public CreateStageCommandHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<StageResult> Handle(CreateStageCommand request, CancellationToken cancellationToken)
    {
        var stage = new ProgressionStage
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            Order = request.Order,
            CreatedAt = _clock.UtcNow,
        };

        _db.ProgressionStages.Add(stage);
        await _db.SaveChangesAsync(cancellationToken);

        return new StageResult(stage.Id);
    }
}

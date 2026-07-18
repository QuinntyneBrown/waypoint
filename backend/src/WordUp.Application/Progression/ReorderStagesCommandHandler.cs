using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;

namespace WordUp.Application.Progression;

public class ReorderStagesCommandHandler : IRequestHandler<ReorderStagesCommand>
{
    private readonly IAppDbContext _db;

    public ReorderStagesCommandHandler(IAppDbContext db) => _db = db;

    public async Task Handle(ReorderStagesCommand request, CancellationToken cancellationToken)
    {
        var ids = request.OrderedStageIds ?? Array.Empty<Guid>();
        var stages = await _db.ProgressionStages
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);

        for (var i = 0; i < ids.Count; i++)
        {
            var stage = stages.FirstOrDefault(s => s.Id == ids[i]);
            if (stage is not null)
            {
                stage.Order = i;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}

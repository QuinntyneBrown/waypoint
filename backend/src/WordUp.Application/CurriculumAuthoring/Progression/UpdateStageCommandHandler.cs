using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public sealed class UpdateStageCommandHandler : IRequestHandler<UpdateStageCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public UpdateStageCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateStageCommand request, CancellationToken cancellationToken)
    {
        var stage = await _db.ProgressionStages.FirstOrDefaultAsync(
            item => item.Id == request.StageId,
            cancellationToken)
            ?? throw new NotFoundException("Progression stage not found.");

        stage.Name = request.Name.Trim();
        stage.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ProgressionStageUpdated",
            _currentUser.UserId,
            "ProgressionStage",
            stage.Id,
            cancellationToken: cancellationToken);
    }
}

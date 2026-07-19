using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public sealed record GetProgressionQuery : IRequest<IReadOnlyList<ProgressionStageView>>;

public sealed record ProgressionStageView(
    Guid Id,
    string Name,
    string? Description,
    int Order,
    IReadOnlyList<ProgressionCompetencyView> Competencies);

public sealed record ProgressionCompetencyView(Guid Id, string Name, int Order);

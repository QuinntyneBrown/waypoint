using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public record CreateStageCommand(string Name, string? Description, int Order) : IRequest<StageResult>;

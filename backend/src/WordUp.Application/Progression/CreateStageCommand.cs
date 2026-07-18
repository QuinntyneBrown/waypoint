using MediatR;

namespace WordUp.Application.Progression;

public record CreateStageCommand(string Name, string? Description, int Order) : IRequest<StageResult>;

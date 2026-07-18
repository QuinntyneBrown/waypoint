using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public record PublishModuleCommand(Guid ModuleId, PublishStatus Status) : IRequest;

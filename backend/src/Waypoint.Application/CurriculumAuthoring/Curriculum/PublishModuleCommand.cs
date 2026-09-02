using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public record PublishModuleCommand(Guid ModuleId, PublishStatus Status) : IRequest;

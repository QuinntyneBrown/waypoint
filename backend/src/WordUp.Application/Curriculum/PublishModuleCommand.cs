using MediatR;
using WordUp.Domain;

namespace WordUp.Application.Curriculum;

public record PublishModuleCommand(Guid ModuleId, PublishStatus Status) : IRequest;

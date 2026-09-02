using MediatR;

namespace Waypoint.Application.ConsentAndSafeguarding;

public record ConsentStatusQuery : IRequest<ConsentStatusResult>;

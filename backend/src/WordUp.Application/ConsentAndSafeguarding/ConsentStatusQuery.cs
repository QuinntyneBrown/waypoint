using MediatR;

namespace WordUp.Application.ConsentAndSafeguarding;

public record ConsentStatusQuery : IRequest<ConsentStatusResult>;

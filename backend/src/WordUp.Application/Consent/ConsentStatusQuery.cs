using MediatR;

namespace WordUp.Application.Consent;

public record ConsentStatusQuery : IRequest<ConsentStatusResult>;
